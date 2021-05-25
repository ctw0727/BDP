using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using BS.Enemy.Boss;
using BS.Manager.Cameras;
using UnityEngine;

namespace BS.Player{
	public class PlayerController : MonoBehaviour
	{
		protected PlayerAnimController _animController;
		protected PlayerPhysicController _physicManager;
		protected ctw_Effector_behavior _effector;

		protected Rigidbody2D _rigid;
		protected Collider2D _collider;

		public GameObject _eraserPrefab;
		public float _maxPower = 2000; /// 최대 차지 파워
		[ProgressBar("Attack Power", 2000, EColor.Green)]
		public float _currentPower = 0; /// 현재 차지 파워
		
		public Animator _bodyAnimator;
		public Animator _faceAnimator;
		protected BulletEraser _eraser;
		
		public PhysicsMaterial2D _normal;
		public PhysicsMaterial2D _bouncy;
		
		Camera _mainCamera;
		
		public float _moveDirection;
		public bool _down = false;
		public int _onHit = 0;
		public int _health = 100;
		public bool _isInvincible = false;
		public bool _isCharging = false;
		public bool _isDead = false;
		public bool _attackSuccess = false;
		[SerializeField]
		private bool _isControllable = true;

		#region get, set
		public PlayerPhysicController PhysicManager{
			get{
				return _physicManager;
			}
		}

		public PlayerAnimController AnimController{
			get{
				return _animController;
			}
		}

		public bool IsControllable {
			get{
				return _isControllable;
			}
			set{
				_isControllable = value;
			}
		}
		#endregion

		#region 플레이어 컨트롤러 초기화
		void Awake(){
			_rigid = GetComponent<Rigidbody2D>();
			_collider = GetComponent<CapsuleCollider2D>() as Collider2D;

			// Init animation controller
			_animController = GetComponent<PlayerAnimController>() ?? this.transform.gameObject.AddComponent<PlayerAnimController>();
			_animController.Init(this, _bodyAnimator, _faceAnimator);

			// Init Physic Manager
			_physicManager = GetComponent<PlayerPhysicController>() ?? this.transform.gameObject.AddComponent<PlayerPhysicController>();
			_physicManager.Init(this, _rigid);

			_effector = FindObjectOfType<ctw_Effector_behavior>();
			_eraser = BulletEraser.Create(_eraserPrefab, this.gameObject);
			
			if(_eraser == null){
				Debug.LogError("eraser가 할당되어 있지 않습니다.");
			}
			if(_effector == null){
				Debug.LogError("effector가 할당되어 있지 않습니다.");
			}

			_mainCamera = CameraManager.Instance.MainCamera;
		}
		#endregion
		
		// Maths
		#region 계산 관련 함수
		float Math_2D_Force(float x, float y){
			return Mathf.Sqrt(Mathf.Pow(x,2)+Mathf.Pow(y,2));
		}
		
		float Math_Boss_Damage(){
			return (500f + Mathf.Abs((_currentPower * Math_2D_Force(_rigid.velocity.x, _rigid.velocity.y) / 50f)) );
		}
		#endregion
		
		// Timers
		void TimerAttackReset(){
			_onHit = 0;
		}

		/// <summary>
		/// 타격을 받은 후 무적 시간을 가짐
		/// </summary>
		/// <param name="t">지속시간</param>
		/// <returns></returns>
		IEnumerator Invicible(float t){
			float duration = t;

			while(duration > 0){
				duration -= Time.deltaTime;
				yield return null;
			}
			_isInvincible = false;
			yield return null;
		}
		
		// Gets
		Vector2 GetForceDirection(){
			
			Vector3 PlayerPos = this.transform.position;
			Vector3 MouseStaticPos = new Vector3(_mainCamera.ScreenToWorldPoint(Input.mousePosition).x,_mainCamera.ScreenToWorldPoint(Input.mousePosition).y,0);
			Vector3 MousePrivatePos = MouseStaticPos - PlayerPos;
			
			float RangeKey = Math_2D_Force(MousePrivatePos.x,MousePrivatePos.y);
			
			MousePrivatePos = MousePrivatePos/RangeKey;
			
			Vector2 ForceDirection = new Vector2(MousePrivatePos.x,MousePrivatePos.y);
			
			return ForceDirection;
		}
		
		float Get_Angle_byPosition(Vector3 Target, Vector3 Pos){
			return ( Mathf.Atan2(Target.y-Pos.y, Target.x-Pos.x) * Mathf.Rad2Deg );
		}
		
		Vector2 Get_Force_byAngle(float angle){
			return new Vector2( Mathf.Cos(angle*Mathf.Deg2Rad), Mathf.Sin(angle*Mathf.Deg2Rad) ); 
		}

		#region Get 함수들
		public bool OnAir(){
			return _physicManager._onAir;
		}

		public bool IsMoving(){
			return _physicManager._isMoving;
		}
		
		public bool IsFalling(){
			return _physicManager._isFalling;
		}
		#endregion

		// Checks
		void OnDamage(){
			if (_health > 1){
				_health -= 1;
				_isInvincible = true;
				_eraser.EraserWave(0.05f);
				_animController.OnHit();

				// 혹시 모를 예외처리
				if(_isInvincible){
					StartCoroutine(Invicible(1.0f));
				}
				CameraManager.Instance.ShakeCamera(0.5f, 0.05f);
			}
			else if (_health == 1){
				_health = 0;
				_isDead = true;
				_animController.Dead();
				CameraManager.Instance.ShakeCamera(1f, 0.1f);
			}
		}
		
		// Effects
		void GenEffect(float angle, float F, float time, int num){
			Vector3 pos = this.transform.position;
			_effector?.Effect_Run(time, pos, Get_Force_byAngle(angle)*F, num);
		}
		
		// Inputs
		
		void InputMove(){
			_moveDirection = 0;
			
			if (_onHit == 0){
				
				if (Input.GetKey(KeyCode.A)){
					_moveDirection--;
					_physicManager._isMoving = true;
				}
				else if (Input.GetKey(KeyCode.D)){
					_moveDirection++;
					_physicManager._isMoving = true;
				}
                else
					_physicManager._isMoving = false;
					
				
				if (Mathf.Abs(_rigid.velocity.x) < 15)
				{
					_rigid.velocity = new Vector2(_rigid.velocity.x+1*_moveDirection,_rigid.velocity.y);
				}
				
				if (Input.GetKey(KeyCode.S)){
					_down = true;
				}
				else{
					_down = false;
				}
				
				if ((Input.GetKeyDown(KeyCode.W)||Input.GetKeyDown(KeyCode.Space))&& !_physicManager._onAir ){
					_rigid.velocity = new Vector2(_rigid.velocity.x,40);
					GenEffect(0f, 30f, 1f, 4);
					GenEffect(180f, 30f, 1f, 4);
					_physicManager._onAir = true;
				}


				//Debug.Log(ismoving);
			}
		}
		
		float AngleBetweenTwoPoints(Vector3 a, Vector3 b){
			return Mathf.Atan2(a.y - b.y, a.x - b.x) * Mathf.Rad2Deg;
		}

		void InputAttack(){

			if(Input.GetMouseButtonDown(0) && _physicManager._onAir && (_onHit != 2)){
				_currentPower = 0;
				_isCharging = true;
				_onHit = 1;
			}
			
			if ((Input.GetMouseButton(0)) && _physicManager._onAir && (_onHit != 2)){
				Vector2 pos = this.transform.position;
				Vector2 mouseOnWorld = _mainCamera.ScreenToWorldPoint(Input.mousePosition);

				float angle = AngleBetweenTwoPoints(pos, mouseOnWorld);

				transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, angle));

				float amount = _maxPower * 0.015f;
				float direction = (mouseOnWorld - pos).x <= 0 ? 1 : -1;

				_currentPower += amount * direction;
			}

			if ((Input.GetMouseButtonUp(0)) && (_onHit == 1)){
				if (Mathf.Abs(_currentPower) >= _maxPower){
					_currentPower = (_currentPower > 0 ? 1 : -1) * _maxPower;
				}

				_rigid.velocity = GetForceDirection() * Mathf.Abs(_currentPower) / 40;
				_collider.sharedMaterial = _bouncy;

				_onHit = 2;
				_isCharging = false;
			}
			
			if (_onHit == 1){
				_rigid.angularDrag = 0.1f;
				_rigid.drag = 2.5f;
				_rigid.gravityScale = 0.5f;
			}
			
			else{
				_rigid.angularDrag = 0.2f;
				_rigid.drag = 0.2f;
				_rigid.gravityScale = 9.8f;
			}
			
			if (_onHit != 2)
				_collider.sharedMaterial = _normal;
		}
		
		
		
		// Running
		
		void Caring(){
			
			if (_physicManager._onAir && (_onHit != 1)){
				_rigid.gravityScale = 9.8f;
			}
			
			if ( !_physicManager._onAir ){
				_rigid.gravityScale = 4.9f;
			}
		}
		
		public void ProcessEffect(Collider2D other){
			// 땅체크는 Layer로 하고 _isCharging 중단 체크는 tag로 하기 때문에
			// 땅의 tag가 정해져 있지 않으면 문제가 생기는 경우가 있음
			// Physics Controller로 옮겨야 하나
			// 어떻게 해야할지 고민 중에 있음
			switch (other.tag){
				case "Platform":
					ctw_Platform_behavior PlatformScript = other.GetComponent<ctw_Platform_behavior>();
					
					if ( (PlatformScript.Trigger == false) && (_rigid.velocity.y <= 0) ){
						if ( _physicManager._onAir ){
							GenEffect(0f, 15f, 1f, 3);
							GenEffect(180f, 15f, 1f, 3);
						}
						_attackSuccess = false;
						_isCharging = false;
					}
					break;
				case "Ground":
					if ( _physicManager._onAir && (_rigid.velocity.y <= -1f)){
						GenEffect(0f, 15f, 1f, 3);
						GenEffect(180f, 15f, 1f, 3);
					}
					_attackSuccess = false;
					_isCharging = false;
					break;
			}
		}

		void OnTriggerStay2D(Collider2D other){
			switch (other.tag){
				case "Bullet":
					if (!_isInvincible){
						ctw_Bullet_Collider_Script BulletScript = other.GetComponent<ctw_Bullet_Collider_Script>();
						if ((!_isDead)&&(BulletScript.OnWork == true)) {
							GenEffect(Get_Angle_byPosition(this.transform.position, other.GetComponent<Transform>().position)+35f, 15f, 1f, 3);
							GenEffect(Get_Angle_byPosition(this.transform.position, other.GetComponent<Transform>().position)-35f, 15f, 1f, 3);
							OnDamage();
						}
						BulletScript.Hitted();
					}
					break;
			}
		}
		
		void OnCollisionEnter2D(Collision2D other){
			if ( (_onHit != 2) && (other.collider.name == "BS_Boss") && (!_isInvincible) ){
				if (!_isDead) {
					GenEffect(Get_Angle_byPosition(this.transform.position, other.collider.GetComponent<Transform>().position), 25f, 2f, 10);
					OnDamage();
				}
			}
			if ( (_onHit == 2) && (other.collider.name == "BS_Boss") ){
				GenEffect(Get_Angle_byPosition(other.collider.GetComponent<Transform>().position, this.transform.position)+60f, 30f, 3f, 8);
				GenEffect(Get_Angle_byPosition(other.collider.GetComponent<Transform>().position, this.transform.position)-60f, 30f, 3f, 8);

				BossBehavior bossScript = other.collider.GetComponent<BossBehavior>();
				bossScript.OnDamaged(Math_Boss_Damage());
				_attackSuccess = true;
				_animController.AttackSuccess();
			}
			
			if (other.collider.tag != "Wall"){
				TimerAttackReset();
			}
		}
		
		private void Update(){
			if (!_isDead && IsControllable){
				InputAttack();
				InputMove();
			}
			else {
				_onHit = 0;
				_rigid.angularDrag = 0.2f;
				_rigid.drag = 0.2f;
				_rigid.gravityScale = 9.8f;
				_collider.sharedMaterial = _normal;
			}
			
			Caring();
			if(_animController != null){
				_animController.Render();
			}
		}
	}
}

