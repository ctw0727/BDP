﻿using System.Collections;
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
		public float _maxPower = 2000; /// 최대 차지 파워
		[ProgressBar("Attack Power", 2000, EColor.Green)]
		public float _currentPower = 0; /// 현재 차지 파워
		
		public Animator _bodyAnimator;
		public Animator _faceAnimator;
		public BulletEraser _eraser;
		
		public PhysicsMaterial2D _normal;
		public PhysicsMaterial2D _bouncy;
		
		Camera _mainCamera;
		
		public float _moveDirection;
		public bool _down = false;
		public int _onHit = 0;
		[ProgressBar("Health", 3, EColor.Red)]
		public int _health = 3;
		public bool isInvincible = false;
		public bool IsCharging = false;
		public bool isDead = false;
		public bool attackSuccess = false;

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
			
			if(_eraser == null){
				Debug.LogError("eraser가 할당되어 있지 않습니다.");
			}
			if(_effector == null){
				Debug.LogError("effector가 할당되어 있지 않습니다.");
			}

			_mainCamera = CameraManager.Instance.MainCamera;
		}
		
		// Maths
		
		float Math_2D_Force(float x, float y){
			return Mathf.Sqrt(Mathf.Pow(x,2)+Mathf.Pow(y,2));
		}
		
		float Math_Boss_Damage(){
			return (500f + Mathf.Abs((_currentPower * Math_2D_Force(_rigid.velocity.x, _rigid.velocity.y) / 50f)) );
		}
		
		// Timers
		void TimerAttackReset(){
			_onHit = 0;
		}
		
		void TimerInvincibleReset(){
			isInvincible = false;
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
				isInvincible = true;
				_eraser.EraserWave(0.05f);
				_animController.OnHit();
				Invoke("TimerInvincibleReset",1.0f);
				CameraManager.Instance.ShakeCamera(0.5f, 0.05f);
			}
			else if (_health == 1){
				_health = 0;
				isDead = true;
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
				IsCharging = true;
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

			if ((Input.GetMouseButtonUp(0))&&(_onHit == 1)){
				if (Mathf.Abs(_currentPower) >= _maxPower){
					_currentPower = (_currentPower > 0 ? 1 : -1) * _maxPower;
				}

				_rigid.velocity = GetForceDirection() * Mathf.Abs(_currentPower) / 40;
				_collider.sharedMaterial = _bouncy;

				_onHit = 2;
				IsCharging = false;
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
			switch (other.tag){
				case "Platform":
					ctw_Platform_behavior PlatformScript = other.GetComponent<ctw_Platform_behavior>();
					
					if ((PlatformScript.Trigger == false)&&(_rigid.velocity.y <= 0)){
						if ( _physicManager._onAir ){
							GenEffect(0f, 15f, 1f, 3);
							GenEffect(180f, 15f, 1f, 3);
						}
						attackSuccess = false;
						IsCharging = false;
					}
					break;
				case "Ground":
					if ( _physicManager._onAir && (_rigid.velocity.y <= -1f)){
						GenEffect(0f, 15f, 1f, 3);
						GenEffect(180f, 15f, 1f, 3);
					}
					attackSuccess = false;
					IsCharging = false;
					break;
			}
		}

		void OnTriggerStay2D(Collider2D other){
			switch (other.tag){
				case "Bullet":
					if (!isInvincible){
						ctw_Bullet_Collider_Script BulletScript = other.GetComponent<ctw_Bullet_Collider_Script>();
						if ((!isDead)&&(BulletScript.OnWork == true)) {
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
			if ( (_onHit != 2) && (other.collider.name == "BS_Boss") && (!isInvincible) ){
				if (!isDead) {
					GenEffect(Get_Angle_byPosition(this.transform.position, other.collider.GetComponent<Transform>().position), 25f, 2f, 10);
					OnDamage();
				}
			}
			if ( (_onHit == 2) && (other.collider.name == "BS_Boss") ){
				GenEffect(Get_Angle_byPosition(other.collider.GetComponent<Transform>().position, this.transform.position)+60f, 30f, 3f, 8);
				GenEffect(Get_Angle_byPosition(other.collider.GetComponent<Transform>().position, this.transform.position)-60f, 30f, 3f, 8);

				BossBehavior bossScript = other.collider.GetComponent<BossBehavior>();
				bossScript.OnDamaged(Math_Boss_Damage());
				attackSuccess = true;
				_animController.AttackSuccess();
			}
			
			if (other.collider.tag != "Wall"){
				TimerAttackReset();
			}
		}
		
		private void Update(){
			if (!isDead){
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

