using System.Collections;
using BS.Camera;
using BS.Enemy.Boss;
using BS.Model;
using BS.Runtime.Extensions;
using BS.Runtime.Input;
using BS.SO;
using R3;
using Reflex.Core;
using Reflex.Extensions;
using UnityEngine;

namespace BS.Player
{
	public class PlayerController : MonoBehaviour
	{
		const float MoveAcceleration = 1f;
		const float JumpSpeed = 40f;
		const float ChargeRate = 0.1f;

		[SerializeField] CharacterSO _character;
		[SerializeField] PhysicsMaterial2D _normal;
		[SerializeField] PhysicsMaterial2D _bouncy;

		PlayerPhysicController _physic;
		CharacterModel _model;
		ctw_Effector_behavior _effector;
		BulletEraser _eraser;
		CameraService _cameraService;

		public GameObject _eraserPrefab;
		public float _maxPower = 2000f;
		public float _currentPower;
		public float _moveDirection;
		public bool _down;
		public int _health = 100;
		public bool _isInvincible;
		public bool _isCharging;
		public bool _isDead;
		public bool _attackSuccess;

		[SerializeField] bool _isControllable = true;

		Vector2 _moveInput;
		bool _isSlamming;
		bool _isBouncy;
		float _slamPower;
		float _aimAngle;
		int _facingX = 1;

		public PlayerPhysicController PhysicManager => _physic;
		public bool IsControllable
		{
			get => _isControllable;
			set => _isControllable = value;
		}

		public Vector2 Position => _physic != null ? _physic.Position : (Vector2)transform.position;

		bool CanControl => !_isDead && _isControllable;

		float MaxMoveSpeed => _character != null ? _character.moveSpeed : 15f;

		void Awake()
		{
			DisableLegacyRenderers();
			_model = new CharacterModel(_character);
			_physic = GetComponent<PlayerPhysicController>() ?? gameObject.AddComponent<PlayerPhysicController>();
			_physic.Init(this);
			_effector = FindAnyObjectByType<ctw_Effector_behavior>();
			_eraser = BulletEraser.Create(_eraserPrefab, gameObject);
		}

		void Start()
		{
			SubscribeInputEvents();
		}

		void Update()
		{
			if (_model == null)
				return;

			if (CanControl)
			{
				UpdateAttackPhysics();
				UpdateMove();
			}
			else
			{
				ResetControlState();
			}

			UpdateGravity();

			CharacterFrame frame = new CharacterFrame
			{
				position = Position,
				velocity = _physic.LinearVelocity,
				rollRadius = _physic.RollRadius,
				isCharging = _isCharging,
				isFalling = _physic._isFalling,
				attackSuccess = _attackSuccess,
				chargeProgress = CharacterModel.ChargeProgress(_currentPower, _maxPower),
				aimAngle = _aimAngle,
				facingX = _facingX
			};
			_model.Tick(Time.deltaTime, frame);
			_model.Render();
		}

		public void PowerOff()
		{
			_model?.Off();
		}

		public void PowerOn()
		{
			_model?.On();
		}

		public bool OnAir()
		{
			return _physic._onAir;
		}

		public bool IsFalling()
		{
			return _physic._isFalling;
		}

		public virtual void OnHit()
		{
			ApplyDamage();
		}

		public void OnBulletHit(Vector2 bulletPosition)
		{
			if (_isInvincible || _isDead)
				return;

			float angle = Angle(bulletPosition, Position);
			Burst(angle + 35f, 15f, 1f, 3);
			Burst(angle - 35f, 15f, 1f, 3);
			ApplyDamage();
		}

		public void OnSurfaceContact(GameObject other)
		{
			if (other == null)
				return;

			if (other.CompareTag("Platform"))
			{
				ctw_Platform_behavior platform = other.GetComponent<ctw_Platform_behavior>();
				if (platform != null && platform.Trigger)
					return;
				if (_physic.LinearVelocity.y > 0f)
					return;

				Land(playDust: _physic._onAir);
				return;
			}

			if (!other.CompareTag("Ground"))
				return;

			Land(playDust: _physic._onAir && _physic.LinearVelocity.y <= -1f);
			_currentPower = 0f;
		}

		public void OnPhysicsContact(GameObject other)
		{
			if (other == null)
				return;

			if (other.TryGetComponent<BaseBossBehavior>(out BaseBossBehavior boss))
				ResolveBossContact(boss, other.transform.position);

			if (!other.CompareTag("Wall"))
				EndAttack();
		}

		void SubscribeInputEvents()
		{
			Container sceneContainer = gameObject.scene.GetSceneContainer();
			if (sceneContainer.TryResolve<InputService>(out InputService inputService))
			{
				inputService.OnPlayerMove
					.Subscribe(OnMovePerformed)
					.AddTo(this);

				inputService.OnPlayerJump
					.Subscribe(OnJumpPerformed)
					.AddTo(this);

				inputService.OnPlayerCharge
					.Subscribe(OnChargePerformed)
					.AddTo(this);

				inputService.OnPlayerChargeRelease
					.Subscribe(OnChargeReleasePerformed)
					.AddTo(this);

				inputService.SetPlayerInputEnable(true);
			}

			if (sceneContainer.TryResolve<CameraService>(out CameraService cameraService))
				_cameraService = cameraService;
		}

		void OnMovePerformed(Vector2 input)
		{
			bool jumpPressed = input.y > 0.01f && _moveInput.y <= 0.01f;
			_moveInput = input;

			if (jumpPressed)
				Jump();
		}

		void OnJumpPerformed(Unit _)
		{
			Jump();
		}

		void OnChargePerformed(Vector2 chargePoint)
		{
			if (!_isCharging)
			{
				BeginCharge();
			}

			_isCharging = true;
			HoldCharge(chargePoint);
		}

		void OnChargeReleasePerformed(Vector2 chargePoint)
		{
			ReleaseCharge(chargePoint);
			_isCharging = false;
		}

		void BeginCharge()
		{
			if (!CanControl || !_physic._onAir || _isSlamming)
				return;

			_currentPower = 0f;
			_isCharging = true;
		}

		void HoldCharge(Vector2 chargePoint)
		{
			if (!_isCharging || !CanControl || !_physic._onAir)
				return;

			Vector2 position = Position;
			bool mouseOnLeft = (chargePoint - position).x <= 0f;

			_aimAngle = Angle(chargePoint, position);
			_facingX = mouseOnLeft ? -1 : 1;
			_currentPower += _maxPower * ChargeRate * (mouseOnLeft ? 1f : -1f);
		}

		void ReleaseCharge(Vector2 chargePoint)
		{
			if (!_isCharging)
				return;

			_isCharging = false;
			if (!CanControl)
				return;

			if (Mathf.Abs(_currentPower) >= _maxPower)
				_currentPower = Mathf.Sign(_currentPower) * _maxPower;

			_slamPower = Mathf.Abs(_currentPower);
			_isSlamming = true;
			_physic.LinearVelocity = (chargePoint - Position).normalized * _slamPower;
			SetBouncy(true);
		}

		void UpdateMove()
		{
			_moveDirection = 0f;
			if (_isCharging || _isSlamming)
				return;

			if (_moveInput.x < -0.01f)
				_moveDirection = -1f;
			else if (_moveInput.x > 0.01f)
				_moveDirection = 1f;

			_physic._isMoving = _moveDirection != 0f;
			if (_physic._isMoving)
				_facingX = (int)_moveDirection;

			Vector2 velocity = _physic.LinearVelocity;
			if (Mathf.Abs(velocity.x) < MaxMoveSpeed)
				_physic.LinearVelocity = new Vector2(velocity.x + MoveAcceleration * _moveDirection, velocity.y);

			_down = _moveInput.y < -0.01f;
		}

		void UpdateAttackPhysics()
		{
			if (_isCharging)
			{
				_physic.AngularDamping = 0.1f;
				_physic.LinearDamping = 2.5f;
				_physic.GravityScale = 0.5f;
			}
			else
			{
				_physic.AngularDamping = 0.2f;
				_physic.LinearDamping = 0.2f;
				_physic.GravityScale = 9.8f;
			}

			if (!_isSlamming)
				SetBouncy(false);
		}

		void UpdateGravity()
		{
			if (_physic._onAir && !_isCharging)
				_physic.GravityScale = 9.8f;

			if (!_physic._onAir)
				_physic.GravityScale = 4.9f;
		}

		void ResetControlState()
		{
			_isCharging = false;
			_isSlamming = false;
			_moveDirection = 0f;
			_physic._isMoving = false;
			_physic.AngularDamping = 0.2f;
			_physic.LinearDamping = 0.2f;
			_physic.GravityScale = 9.8f;
			SetBouncy(false);
		}

		void SetBouncy(bool bouncy)
		{
			if (_isBouncy == bouncy)
				return;

			_isBouncy = bouncy;
			PhysicsMaterial2D material = bouncy ? _bouncy : _normal;
			float friction = material != null ? material.friction : 0.4f;
			float bounciness = material != null ? material.bounciness : (bouncy ? 0.01f : 0f);
			_physic.SetSurface(friction, bounciness);
		}

		void Jump()
		{
			if (!CanControl || _physic._onAir || _isCharging || _isSlamming)
				return;

			Vector2 velocity = _physic.LinearVelocity;
			_physic.LinearVelocity = new Vector2(velocity.x, JumpSpeed);
			Burst(0f, 30f, 1f, 4);
			Burst(180f, 30f, 1f, 4);
			_physic._onAir = true;
		}

		void Land(bool playDust)
		{
			if (playDust)
			{
				Burst(0f, 15f, 1f, 3);
				Burst(180f, 15f, 1f, 3);
			}

			_attackSuccess = false;
			_isCharging = false;
			_isSlamming = false;
		}

		void EndAttack()
		{
			_isSlamming = false;
			_isCharging = false;
		}

		void ResolveBossContact(BaseBossBehavior boss, Vector2 bossPosition)
		{
			if (_isSlamming)
			{
				StrikeBoss(boss, bossPosition);
				return;
			}

			if (_isInvincible || _isDead)
				return;

			Burst(Angle(bossPosition, Position), 25f, 2f, 10);
			ApplyDamage();
		}

		void StrikeBoss(BaseBossBehavior boss, Vector2 bossPosition)
		{
			float angle = Angle(Position, bossPosition);
			Burst(angle + 60f, 30f, 3f, 8);
			Burst(angle - 60f, 30f, 3f, 8);
			boss.OnDamaged(SlamDamage());
			_attackSuccess = true;
			_model.AttackSuccess();
		}

		void ApplyDamage()
		{
			if (_isInvincible || _isDead)
				return;

			if (_health > 1)
			{
				_health -= 1;
				_isInvincible = true;
				_eraser?.EraserWave(0.05f);
				_model.OnHit(_isCharging);
				StartCoroutine(EndInvincible(1f));
				_cameraService?.ShakeCamera(0.5f, 0.05f);
				return;
			}

			_health = 0;
			_isDead = true;
			_model.Dead();
			_cameraService?.ShakeCamera(1f, 0.1f);
		}

		IEnumerator EndInvincible(float duration)
		{
			float remaining = duration;
			while (remaining > 0f)
			{
				remaining -= Time.deltaTime;
				yield return null;
			}

			_isInvincible = false;
		}

		float SlamDamage()
		{
			Vector2 velocity = _physic.LinearVelocity;
			float speed = Mathf.Sqrt(velocity.x * velocity.x + velocity.y * velocity.y);
			return 500f + _slamPower * speed / 50f;
		}

		void Burst(float angleDegrees, float force, float time, int count)
		{
			Vector2 direction = new Vector2(Mathf.Cos(angleDegrees * Mathf.Deg2Rad), Mathf.Sin(angleDegrees * Mathf.Deg2Rad));
			_effector?.Effect_Run(time, Position, direction * force, count);
		}

		/// <summary>
		/// from에서 to를 향하는 각도(도)
		/// </summary>
		static float Angle(Vector2 from, Vector2 to)
		{
			return Mathf.Atan2(to.y - from.y, to.x - from.x) * Mathf.Rad2Deg;
		}

		void DisableLegacyRenderers()
		{
			SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);
			for (int i = 0; i < renderers.Length; i++)
				renderers[i].enabled = false;
		}
	}
}
