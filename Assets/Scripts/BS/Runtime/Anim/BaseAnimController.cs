using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BS.Anim
{
    /// <summary>
    /// 애니메이션 컨트롤러 인터페이스
    /// </summary>
    public abstract class BaseAnimController : MonoBehaviour
    {
        protected bool isBlink = false;
        protected bool isTween = false;

        public abstract void Init();
        public abstract void Render();

        #region 유틸 함수들
        /// <summary>
        /// alpha 값을 변화시켜 일정 주기로 깜빡거리는 효과를 만듭니다.
        /// </summary>
        /// <param name="t">지속시간</param>
        /// <param name="freq">주기</param>
        /// <param name="sprite">alpha를 변화시킬 SpriteRenderer</param>
        /// <returns></returns>
        protected virtual IEnumerator SpriteAlphaBlinkLoop(SpriteRenderer sprite, float t, float freq){
            float duration = t;
            isBlink = true;
            while(duration > 0){
                float x = (t - duration);
                float alpha = Mathf.Abs(Mathf.Cos((Mathf.PI / freq) * x));
                SetSpriteAlpha(sprite, alpha);

                duration -= Time.deltaTime;
                yield return null;
            }
        
            isBlink = false;
            SetSpriteAlpha(sprite, 1f); // 원래 alpha 값으로 원복
            yield return null;
        }

        protected virtual IEnumerator SpriteAlphaTween(SpriteRenderer sprite, float t){
            float duration = t;
            isTween = true;
            while(duration > 0){
                duration -= Time.deltaTime;
                SetSpriteAlpha(sprite, 1 - (duration / t));
                yield return null;
            }

            isTween = false;
            SetSpriteAlpha(sprite, 1f);
            yield return null;
        }

        
        /// <summary>
        /// sprite의 alpha를 set합니다.
        /// </summary>
        /// <param name="alpha"> set할 alpha값 </param>
        protected virtual void SetSpriteAlpha(SpriteRenderer sprite, float alpha){
            sprite.color = new Color(   sprite.color.r,
                                        sprite.color.g,
                                        sprite.color.b,
                                        alpha);
        }
        #endregion
    }
}
