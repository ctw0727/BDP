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
        [SerializeField] CharacterSO _character;

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

        bool _isSlamming;
        float _slamPower;
        int _facingX = 1;
        Vector2 _lastChargeDirection;

        public PlayerPhysicController PhysicManager => _physic;
        public bool IsControllable
        {
            get => _isControllable;
            set => _isControllable = value;
        }

        public Vector2 Position => _physic != null ? _physic.Position : (Vector2)transform.position;

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

            CharacterFrame frame = new CharacterFrame
            {
                position = Position,
                velocity = _physic.LinearVelocity,
                rollRadius = _physic.RollRadius,
                isCharging = _isCharging,
                isFalling = _physic._isFalling,
                attackSuccess = _attackSuccess,
                chargeProgress = CharacterModel.ChargeProgress(_currentPower, _maxPower),
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
            if (other != null && other.TryGetComponent<BaseBossBehavior>(out BaseBossBehavior boss))
                ResolveBossContact(boss, other.transform.position);
        }

        void SubscribeInputEvents()
        {
            Container sceneContainer = gameObject.scene.GetSceneContainer();
            if (sceneContainer.TryResolve<InputService>(out InputService inputService))
            {
                inputService.OnPlayerMove.Subscribe(OnMovePerformed).AddTo(this);
                inputService.OnPlayerJump.Subscribe(OnJumpPerformed).AddTo(this);
                inputService.OnPlayerCharge.Subscribe(OnChargePerformed).AddTo(this);
                inputService.SetPlayerInputEnable(true);
            }

            if (sceneContainer.TryResolve<CameraService>(out CameraService cameraService))
                _cameraService = cameraService;
        }

        void OnMovePerformed(Vector2 input)
        {
            _moveDirection = input.x;
            if (Mathf.Abs(input.x) > 0.01f)
                _facingX = input.x < 0f ? -1 : 1;

            if (input.sqrMagnitude <= 0f)
            {
                _physic.LinearVelocity = Vector2.zero;
                _physic._isMoving = false;
                return;
            }

            float speed = _character != null ? _character.moveSpeed : _model.MoveSpeed;
            _physic.LinearVelocity = speed * input;
            _physic._isMoving = true;
            if (input.y > 0f)
                Jump();
            else
                _down = input.y < 0f;
        }

        void OnJumpPerformed(Unit _)
        {
            Jump();
        }

        void OnChargePerformed(Vector2 chargePoint)
        {
            if (!_physic._onAir)
                return;

            bool charging = chargePoint != Vector2.zero;
            if (charging)
            {
                _lastChargeDirection = (chargePoint - Position).normalized;
                if (Mathf.Abs(_lastChargeDirection.x) > 0.01f)
                    _facingX = _lastChargeDirection.x < 0f ? -1 : 1;

                _currentPower += _maxPower * 0.015f * _lastChargeDirection.x;
                _physic.AngularDamping = 0.1f;
                _physic.LinearDamping = 2.5f;
                _physic.GravityScale = 0.5f;
                _isCharging = true;
                return;
            }

            if (Mathf.Abs(_currentPower) >= _maxPower)
                _currentPower = Mathf.Sign(_currentPower) * _maxPower;

            _slamPower = Mathf.Abs(_currentPower);
            _isSlamming = _slamPower > 1f;
            _physic.ApplyImpulse(_lastChargeDirection * _slamPower / 40f);
            _isCharging = false;
            _currentPower = 0f;
            _physic.AngularDamping = 0.2f;
            _physic.LinearDamping = 0.2f;
            _physic.GravityScale = 9.8f;
        }

        void Jump()
        {
            if (_physic._onAir)
                return;

            Vector2 velocity = _physic.LinearVelocity;
            _physic.LinearVelocity = new Vector2(velocity.x, 40f);
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

        void ResolveBossContact(BaseBossBehavior boss, Vector2 bossPosition)
        {
            if (_isSlamming)
            {
                StrikeBoss(boss, bossPosition);
                return;
            }

            if (_isInvincible || _isDead)
                return;

            Burst(Angle(Position, bossPosition), 25f, 2f, 10);
            ApplyDamage();
        }

        void StrikeBoss(BaseBossBehavior boss, Vector2 bossPosition)
        {
            float angle = Angle(bossPosition, Position);
            Burst(angle + 60f, 30f, 3f, 8);
            Burst(angle - 60f, 30f, 3f, 8);
            boss.OnDamaged(SlamDamage());
            _attackSuccess = true;
            _isSlamming = false;
            _model.AttackSuccess();
        }

        void ApplyDamage()
        {
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

            if (_health != 1)
                return;

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
