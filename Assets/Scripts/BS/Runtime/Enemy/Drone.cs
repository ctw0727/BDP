using System;
using BS.Projectile;
using BS.Utils;
using UnityEngine;

namespace BS.Enemy{

    public class Drone : MonoBehaviour{
        public GameObject _bulletPrefab;

        private void Awake(){
            for(int i = 0; i < 100; i++){
                var gameObj = Instantiate(_bulletPrefab);
                Bullet bullet = gameObj.GetComponent<Bullet>();
                bullet.transform.SetParent(this.transform);
                bullet.Disable();
            }
        }

        private void Update(){
            if(Input.GetMouseButton(0)){
                var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 dir = new Vector2(mousePos.x, mousePos.y);

                for(int i = 0; i < 100; i++){
                    Bullet bullet = ObjectPool.Instance.Dequeue();

                    if(bullet == null){
                        var gameObj = Instantiate(_bulletPrefab, this.transform.position, this.transform.rotation);
                        bullet = gameObj.GetComponent<Bullet>();
                    }

                    bullet.Init(this.transform.position, 10f);
                    bullet.SetDirection(dir + RandomFunc.RandomVector2(-10f, 10f));
                    bullet.Move();
                }
            }
        }
    }
}
