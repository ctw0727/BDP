using System.Collections;
using BS.Anim;
using BS.Manager.Cameras;
using UnityEngine;

namespace BS.Player
{
    public class PlayerAnimController : BaseAnimController
    {
        // 임시로 public으로 지정
        // Init 함수로 초기화 하는 게 좋을 지도 모름

        /// Animators
        protected PlayerController _player;
        protected Animator _bodyAnimator;
        protected Animator _faceAnimator;

        /// Sprite renders
        protected SpriteRenderer _bodySprite;
        protected SpriteRenderer _faceSprite;

        int prevLeftRightDirection = 1;

        /// <summary>
        /// Base AnimController 초기화
        /// </summary>
        public override void Init(){}

        /// <summary>
        /// 플레이어 Animation Controller 초기화
        /// </summary>
        /// <param name="player"> Player Controller 객체</param>
        /// <param name="bodyAnimator"> body animator</param>
        /// <param name="faceAnimator"> face animator</param>
        public void Init(PlayerController player, Animator bodyAnimator, Animator faceAnimator){
            _player = player;
            _bodyAnimator = bodyAnimator;
            _bodySprite = _bodyAnimator.GetComponent<SpriteRenderer>();
            _faceAnimator = faceAnimator;
            _faceSprite = _faceAnimator.GetComponent<SpriteRenderer>();
        }


        /// <summary>
        /// 플레이어의 sprite alpha를 깜빡거리게 표현합니다.
        /// 삼각함수의 형태로 alpha가 변합니다.
        /// (freq에 0을 할당하지 마세요)
        /// </summary>
        /// <param name="t"> blink 지속 시간</param>
        /// <param name="freq"> alpha가 1이 되는 blink 주기</param>
        public void SpriteAlphaBlink(float t, float freq){
            StartCoroutine(SpriteAlphaBlinkLoop(_bodySprite, t, freq));
            StartCoroutine(SpriteAlphaBlinkLoop(_faceSprite, t, freq));
        }

        /// <summary>
        /// 4 frame 애니메이션으로
        /// 0.75 부터 마지막 프레임이기 때문에
        /// player의 power가 애니메이션의 진행도가 됨
        /// </summary>
        protected void ChargingAnim(){

            if(_player._isCharging){
                float progress = (Mathf.Abs(_player._currentPower) / _player._maxPower) * 0.75f;

                if(progress > 0.75f){
                    progress = 1f;
                }

                _bodyAnimator.SetFloat("Charging", progress);
                _faceAnimator.SetFloat("Charging", progress);
            }else{
                _bodyAnimator.SetFloat("Charging", 0);
                _faceAnimator.SetFloat("Charging", 0);
            }
        }

        /// <summary>
        /// Player 공격 성공시 애니메이션 재생
        /// </summary>
        public void AttackSuccess(){
            _faceAnimator.SetTrigger("AttackSuccess");
        }

        /// <summary>
        /// Player가 공격을 받아 무적 상태에 진입
        /// </summary>
        public void OnHit(){
            if(_player._isInvincible && !isBlink){
                SpriteAlphaBlink(0.9f, 0.03f);

                _bodyAnimator.SetTrigger("OnHit");
                if(!_player._isCharging){
                    _faceAnimator.SetTrigger("OnHit");
                }
            }
        }

        /// <summary>
        /// Player 사망시 애니메이션 재생
        /// </summary>
        public void Dead(){
            if(_player._isDead){
                SetSpriteAlpha(_bodySprite, 0.35f);
                SetSpriteAlpha(_faceSprite, 0.35f);
            }
        }

        public void Off(){
            _bodyAnimator.SetTrigger("BlackNoise");
            _faceSprite.gameObject.SetActive(false);
        }

        public void On(){
            _bodyAnimator.SetTrigger("Idle");
            _faceSprite.gameObject.SetActive(true);
        }
        
        /// <summary>
        /// 이동 방향에 따라 Player sprite를 좌우로 뒤집습니다.
        /// Player의 z rotation이 180이라면 상하로 뒤집습니다.
        /// </summary>
        protected void FlipSprite(){
            float rotationZ = Mathf.Abs(_player.transform.rotation.eulerAngles.z);


            int directionUpDown = 1;
            // Player가 Charging 중...
            if(_player._isCharging){
                if((90 < rotationZ) && (rotationZ < 270)){
                    directionUpDown = -1;
                }
            }
            else if((170 < rotationZ) && (rotationZ < 190)){
                directionUpDown = -1;
            }


            int directionLeftRight = prevLeftRightDirection;
            // Player가 Charging 중...
            if(_player._isCharging){
                Vector2 pos = this.transform.position;
				Vector2 mouseOnWorld = CameraManager.Instance.MainCamera.ScreenToWorldPoint(Input.mousePosition);

                directionLeftRight = (mouseOnWorld - pos).x <= 0 ? -1 : 1;
                prevLeftRightDirection = directionLeftRight;
            }
            else if(Mathf.Abs(_player._moveDirection) > 0){
                directionLeftRight = (int)(_player._moveDirection / Mathf.Abs(_player._moveDirection));
                prevLeftRightDirection = directionLeftRight;
            }

            Vector3 scale = new Vector3(1 * directionLeftRight * directionUpDown, 1 * directionUpDown, 1);

            _faceSprite.transform.localScale = scale;
            _bodySprite.transform.localScale = scale;
        }

        public override void Render(){            
            ChargingAnim();
            
            // Player가 Charging 중이 아니면 Falling Animation
            _faceAnimator.SetBool("Falling", _player.IsFalling() && !_player._isCharging && !_player._attackSuccess);
            
            FlipSprite();
        }
    }
}
