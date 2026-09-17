using System.Collections.Generic;
using BS.UI;
using UnityEngine;


namespace BS.Enemy.Boss
{
    public class BigComputerBoss : BaseBoss
    {
        /// <summary>
        /// BaseBoss 초기화
        /// </summary>
        protected override void Awake()
        {
            IsEnabled = true;
            IsDead = false;
            _healthBar = GetComponent<HealthBar>();

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

        protected override void Update()
        {
            base.Update();
        }

        #region util buttons
        /// <summary>
        /// BFS 탐색 알고리즘으로 탐색하며 단계가 내려갈 때 마다 order도 내려갑니다.
        /// </summary>
        /// <param name="startOrder">시작 order</param>
        void AutoSetOrderLayer(int startOrder = 5)
        {
            Queue<Transform> queue = new Queue<Transform>();

            queue.Enqueue(this.transform);
            while (queue.Count > 0)
            {
                Transform obj = queue.Dequeue();
                int order = startOrder;

                foreach (var child in obj.GetComponentsInChildren<Transform>())
                {
                    if (child.parent == obj)
                    {
                        SpriteRenderer sprite = obj.GetComponent<SpriteRenderer>();
                        if (sprite != null)
                        {
                            order = sprite.sortingOrder - 1;
                        }
                        else
                        {
                            order = startOrder;
                            Debug.LogWarningFormat("{0}의 부모에게 SpriteRenderer가 없습니다. order를 {1}로 합니다.", child.name, order);
                        }

                        child.GetComponent<SpriteRenderer>().sortingOrder = order;
                        queue.Enqueue(child);
                    }
                }
            }
        }
        #endregion
    }
}
