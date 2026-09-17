using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BS.Enemy.Boss{
    public class BigComputerBossAnimController : BaseBossAnimController
    {
        public override void Init(){
            _boss = GetComponent<BaseBoss>();
            // _bossSprite = GetComponent<SpriteRenderer>();
            _boss.BossBehavior.OnHit.AddListener(OnHit);
        }

        public override void Render(){
            
        }
    }
}
