﻿using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

/// <summary>
/// 플레이어의 물리 관련 처리를 위한 컴포넌트
/// </summary>
namespace BS.Player{
    public class PlayerPhysicManager : MonoBehaviour
    {
        [SerializeField]
        private LayerMask _groundLayers;
        [SerializeField]
        private LayerMask _bulletLayers;
        private Transform _groundChecker;
        public float _groundRadius = .2f; // ground를 체크하는 콜라이더의 지름
        
        /* 물리 관련 bool */
        [SerializeField]
        protected bool _isGrounded = false;
		public bool _isFalling = false;
		public bool _onAir = false;
		public bool _isMoving = false;

        private PlayerController _player;
        private Rigidbody2D _rigid;

        public void Init(PlayerController player, Rigidbody2D rigid){
            _player = player;
            _rigid = rigid;
            _groundChecker = new GameObject("GroundChecker").transform;
            _groundChecker.SetParent(this.transform);
        }

        public bool IsGround(){
            return _isGrounded;
        }

        protected void CheckGround(){
            bool wasGrounded = _isGrounded;
            _isGrounded = false;

            Collider2D[] colliders = Physics2D.OverlapCircleAll(_groundChecker.position, _groundRadius, _groundLayers);
            for(int i = 0; i < colliders.Length; i++){
                if(colliders[i].gameObject != gameObject){
                    _isGrounded = true;
                    _player.ProcessEffect(colliders[i]);
                }
            }

            _onAir = false;
            _isFalling = false;
            if(wasGrounded == false && _isGrounded == false){
                _onAir = true;

                if(_rigid.velocity.y < 0){
                    _isFalling = true;
                }
            }
        }

        private void FixedUpdate(){
            _groundChecker.position = this.transform.position + Vector3.down * _groundRadius;
            CheckGround();
        }

        private void OnDrawGizmos(){
            if(_groundChecker != null){
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(_groundChecker.position, _groundRadius);
            }else{
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(this.transform.position + Vector3.down * _groundRadius, _groundRadius);
            }
        }
    }
}
