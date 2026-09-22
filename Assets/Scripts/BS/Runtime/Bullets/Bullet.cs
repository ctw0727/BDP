using System;
using BS.Player;
using BS.Utils;
using UnityEngine;

namespace BS.Projectile
{
	[RequireComponent(typeof(Rigidbody2D))]
	public class Bullet : MonoBehaviour
	{
		public bool _isEnabled = false;
		public bool _isLiveInView = true; // camera view에서만 존재할 것인지 여부


		[SerializeField]
		private LayerMask _targetLayers = 0; // 충돌처리할 Layer

		private float _speed = 0f;
		private Vector2 _dir;


		private Rigidbody2D _rigid;

		public Rigidbody2D Rigid
		{
			get
			{
				return _rigid;
			}
		}

		public float Speed
		{
			get
			{
				return _speed;
			}
			set
			{
				_speed = value;
			}
		}

		public Vector2 Direction
		{
			get
			{
				return _dir;
			}
		}


		#region 초기화 함수들
		/// <summary>
		/// Bullet 객체의 초기화를 담당하는 함수
		/// </summary>
		public void Init()
		{
			Init(Vector2.zero, 0f);
		}

		public void Init(float speed)
		{
			Init(Vector2.zero, speed);
		}

		public void Init(Vector2 pos, float speed)
		{
			Enable();
			this.transform.position = pos;
			this.transform.rotation = Quaternion.Euler(0, 0, 0);
			_speed = speed;
			_dir = Vector2.zero;

			// if(GetComponent<CircleCollider2D>() == null){
			// 	Debug.LogWarning("CircleCollider가 없습니다.");
			// }

			if (_rigid == null)
			{
				_rigid = GetComponent<Rigidbody2D>();
				if (GetComponent<Rigidbody2D>() == null)
				{
					Debug.LogWarning("RigidBody2D가 없습니다.");
				}
			}
		}
		#endregion


		private void Update()
		{
			if (_isLiveInView && _isEnabled)
			{
				Vector3 objScreenPos = UnityEngine.Camera.main.WorldToScreenPoint(this.transform.position);
				if (objScreenPos.x < 0 || objScreenPos.x > Screen.width || objScreenPos.y < 0 || objScreenPos.y > Screen.height)
				{
					Disable();
				}
			}
		}


		/// <summary>
		/// Bullet 이동 방향 Set
		/// </summary>
		/// <param name="dir"></param>
		public void SetDirection(Vector2 dir)
		{
			_dir = dir;
		}


		/// <summary>
		/// Bullet의 Z 각도를 조절합니다. 
		/// </summary>
		/// <param name="rotation"></param>
		public void SetRotation(float rotation)
		{
			this.transform.rotation = Quaternion.AngleAxis(rotation, Vector3.forward);
		}


		/// <summary>
		/// Bullet 이동 처리
		/// </summary>
		public void Move()
		{
			_rigid.linearVelocity = _dir.normalized * _speed;
		}

		public void Disable()
		{
			_isEnabled = false;
			this.gameObject.SetActive(false);
		}

		public void Enable()
		{
			this.gameObject.SetActive(true);
			_isEnabled = true;
		}

		private void OnTriggerEnter2D(Collider2D other)
		{
			if (((1 << other.gameObject.layer) & _targetLayers) != 0)
			{
				other.GetComponent<PlayerController>().OnHit();
				Disable();
			}
		}
	}
}

