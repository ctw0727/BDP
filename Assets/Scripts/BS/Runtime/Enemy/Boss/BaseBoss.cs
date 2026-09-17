using System;
using BS.UI;
using UnityEngine;

namespace BS.Enemy.Boss
{
    public class BaseBoss : MonoBehaviour
    {
        protected BaseBossAnimController _animController;
        protected BaseBossBehavior _bossBehavior;

        protected HealthBar _healthBar;

        protected float _maxHealth;
        public float _health;

        public bool _isDead = false;
        public bool _isEnabled = false;
        public bool _isInvincible = false;

        #region get, set
        public bool IsDead
        {
            get
            {
                return _isDead;
            }
            set
            {
                _isDead = value;
            }
        }
        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                _isEnabled = value;
            }
        }
        public bool IsInvincible
        {
            get
            {
                return _isInvincible;
            }
            set
            {
                _isInvincible = value;
            }
        }
        #endregion

        #region Boss 관련 스크립트 get
        public BaseBossAnimController AnimController
        {
            get
            {
                return _animController;
            }
        }

        public BaseBossBehavior BossBehavior
        {
            get
            {
                return _bossBehavior;
            }
        }
        #endregion

        /// <summary>
        /// BaseBoss 초기화
        /// </summary>
        protected virtual void Awake()
        {
            IsEnabled = true;
            IsDead = false;
            _healthBar = GetComponent<HealthBar>();
            _healthBar?.Init();

            // 순서 바뀌면 안됨 
            // behavior -> anim 순으로 초기화할 것
            _bossBehavior = GetComponent<BaseBossBehavior>();
            if (_bossBehavior == null)
            {
                _bossBehavior = this.gameObject.AddComponent<BaseBossBehavior>();
            }
            _bossBehavior?.Init();

            _animController = GetComponent<BaseBossAnimController>();
            if (_animController == null)
            {
                _animController = this.gameObject.AddComponent<BaseBossAnimController>();
            }
            _animController?.Init();

        }

        protected virtual void Update()
        {
            if (_bossBehavior != null)
            {
                _bossBehavior.Tick();
            }

            if (_animController != null)
            {
                _animController.Render();
            }
        }

        #region Boss 체력관련 함수

        public float Health
        {
            get
            {
                return _health;
            }
        }

        public float MaxHealth
        {
            get
            {
                return _maxHealth;
            }
        }

        /// <summary>
        /// Boss의 최대체력을 조절합니다.
        /// 조절한후 체력을 만땅으로 채웁니다.
        /// </summary>
        /// <param name="maxHealth">설정할 최대체력</param>
        public void SetMaxHealth(float maxHealth)
        {
            _maxHealth = maxHealth;
            _health = maxHealth;
            _healthBar?.SetMaxHealth(_maxHealth);
        }

        /// <summary>
        /// Boss의 체력을 조절합니다.
        /// 최대체력을 넘길 수 없습니다.
        /// </summary>
        /// <param name="health">설정할 최대체력</param>
        public void SetHealth(float health)
        {
            _health = Mathf.Min(_maxHealth, health);
            _healthBar?.SetHealth(health);
        }
        #endregion
    }
}