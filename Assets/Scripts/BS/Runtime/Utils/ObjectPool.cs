using System.Collections;
using System.Collections.Generic;
using BS.Manager;
using BS.Projectile;
using UnityEngine;

namespace BS.Utils
{
    public class ObjectPool : BaseManager<ObjectPool>
    {
        private Queue<Bullet> _pool = new Queue<Bullet>();
        private int _count = 0;

        #region get
        /// <summary>
        /// 생성된 Obj 갯수
        /// </summary>
        public int Count
        {
            get
            {
                return _count;
            }
        }

        /// <summary>
        /// Pool에 들어있는 Obj 갯수
        /// </summary>
        public int PoolCount
        {
            get
            {
                return _pool.Count;
            }
        }
        #endregion

        public void Enqueue(Bullet obj)
        {
            _count++;
            _pool.Enqueue(obj);
        }

        public Bullet Dequeue()
        {
            if (_pool.Count == 0)
            {
                Debug.LogWarning("Pool의 숫자가 부족합니다.");
                return null;
            }

            _count--;
            return _pool.Dequeue();
        }
    }
}
