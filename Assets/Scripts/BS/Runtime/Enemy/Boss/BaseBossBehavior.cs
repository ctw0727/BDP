using BS.BehaviorTrees.Trees;
using BS.Player;
using UnityEngine.Events;
using UnityEngine;

namespace BS.Enemy.Boss{
    
    [RequireComponent(typeof(BaseBoss))]
    public class BaseBossBehavior : MonoBehaviour
    {
        #region event들
        [HideInInspector]
        public UnityEvent OnHit;
        #endregion

        public GameObject _eraserPrefab;

        protected BaseBoss _boss;
        protected PlayerController _player;
        protected BulletEraser _eraser; // 임시로 public
        protected BehaviorTree _tree;

        /// <summary>
        /// Boss Behavior 초기화
        /// </summary>
        public virtual void Init(){
            OnHit = new UnityEvent();
            _boss = GetComponent<BaseBoss>();
            _player = FindObjectOfType<PlayerController>();
            _eraser = BulletEraser.Create(_eraserPrefab, this.gameObject);

            if(_eraser == null){
                Debug.LogWarning("eraser가 할당되지 않았습니다.");
            }
        }

        public virtual void Tick(){
            _tree.Tick();
        }

        public virtual void OnDamaged(float damage){}

        protected virtual void OnDestroy(){
            OnHit.RemoveAllListeners();
        }
    }
}
