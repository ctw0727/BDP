using System.Collections;
using System.Collections.Generic;
using BS.Anim;
using UnityEngine;

namespace BS.Enemy.Boss
{
    public class BaseBossAnimController : BaseAnimController
    {
        protected BaseBoss _boss;
        protected SpriteRenderer _bossSprite;


        public override void Init(){
            _boss = GetComponent<BaseBoss>();
            _bossSprite = GetComponent<SpriteRenderer>();
            _boss.BossBehavior.OnHit.AddListener(OnHit);
        }

        protected void OnHit(){
            if(_boss.IsDead){
                Dead();
            }else{
                Invincible();
            }
        }

        protected void Invincible(){
            StartCoroutine(SpriteAlphaTween(_bossSprite, 1f));
        }

        protected void Dead(){
            SetSpriteAlpha(_bossSprite, 1f);
        }

        public override void Render(){
            
        }
    }
}
