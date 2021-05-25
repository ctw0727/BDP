using System;
using System.Collections;
using System.Collections.Generic;
using BS.BehaviorTrees.Trees;
using BS.BehaviorTrees.Tasks;
using BS.Utils;
using NaughtyAttributes;
using UnityEngine;

namespace BS.Enemy.Boss{

    public class BigComputerBossBehavior : BaseBossBehavior
    {
        [Flags]
        enum AttackType {
            DRONE,
            SHOTGUN,
            LASER,
            EXPLOSION_CORE,
            SMASH
        }

        const int MAX_HEALTH = 10000;
        private AttackType _attackType;

        public LineRenderer _lineRender;
        public List<Transform> _bones;
        public bool _isSmashing;

        public override void Init()
        {
            base.Init();
            _boss.SetMaxHealth(MAX_HEALTH);

            
            #region behavior tree
            _tree = new BehaviorTreeBuilder(gameObject)
                        .Selector()
                            .Condition(() => _boss.IsDead)
                            .Condition(() => _boss.IsInvincible)
                            .Sequence()
                                .Selector("Attack")
                                    .Sequence("Drone")
                                        .Condition(() => _attackType == AttackType.DRONE)
                                        .Do("Drone Attack", () => {
                                                DroneAttack();
                                                return TaskStatus.Success;
                                            })
                                        .WaitTime(0.8f)
                                    .End()
                                    .Sequence("Shotgun")
                                        .Condition(() => _attackType == AttackType.SHOTGUN)
                                        .Do("Fire Shotgun", () => {
                                                FireShotgun();
                                                return TaskStatus.Success;
                                            })
                                        .WaitTime(1.5f)
                                    .End()
                                    .Sequence("Laser")
                                        .Condition(() => _attackType == AttackType.LASER)
                                        .Do("Fire Laser", () => {
                                                Laser(Vector2.right);
                                                return TaskStatus.Success; 
                                            })
                                        .WaitTime(3f)
                                    .End()
                                    .Sequence("ExplosionCore")
                                        .Condition(() => _attackType == AttackType.EXPLOSION_CORE)
                                        .Do("Fire Explosion Core", () => {
                                                FireExplosionCore();
                                                return TaskStatus.Success; 
                                            })
                                        .WaitTime(0.8f)
                                    .End()
                                    .Sequence("Smash")
                                        .Condition(() => _attackType == AttackType.SMASH)
                                        .Do("Smash Player", () => {
                                                Smash();
                                                return TaskStatus.Success; 
                                            })
                                        .WaitTime(3f)
                                    .End()
                                .End()
                                .Do("Choose Random Attack Type", () => {
                                    // _attackType = BS.Utils.Random.RandomEnum<AttackType>();
                                    _attackType = AttackType.SMASH;
                                    return TaskStatus.Success;
                                })
                            .End()
                    .Build();
            #endregion
        }

        [Button("라인 만들기")]
        public void DrawLine(){
            if(_bones.Count > 0){
                _bones.Clear();
            }
            var childs = GetComponentsInChildren<Transform>();

            foreach(var child in childs){
                if(child.parent == this.transform){
                    _bones.Add(child);
                }
            }

            _lineRender.positionCount = _bones.Count;
            int index = 0;
            foreach(var obj in _bones){
                _lineRender.SetPosition(index, obj.position);
                index++;
            }
        }

        #region 공격패턴 1단계

        #region  Drone Attack
        private void DroneAttack(){
            Debug.Log("드론 공격");
        }
        #endregion

        #region Fire Shotgun
        private void FireShotgun(){
            Debug.Log("샷건 발사");
        }
        #endregion

        #region Laser Attack
        private void Laser(Vector2 axis){
            Debug.Log("레이저 공격");
        }
        #endregion

        #region Explosion Core
        private void FireExplosionCore(){
            Debug.Log("폭발 코어 공격");
        }   
        #endregion

        #region  Smashing
        private void Smash(){
            Debug.Log("스메쉬");
            Vector3 dest = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            dest = new Vector3(dest.x, dest.y, 0);
            StartCoroutine(SmashLoop(dest, 0.5f, 0.5f, 0.5f, 0.5f));
        }

        /// <summary>
        /// 스메쉬 공격
        /// </summary>
        /// <param name="dest">목표 좌표</param>
        /// <param name="t">
        ///  t[0]: 위로 올라가는 시간
        /// 
        ///  t[1]: 목표 x좌표로 이동 시간
        /// 
        ///  t[2]: 약간 위로 올릴 때 걸리는 시간
        /// 
        ///  t[3]: 내려찍는데 걸리는 시간
        /// </param>
        /// <returns></returns>
        IEnumerator SmashLoop(Vector3 dest, params float[] t){
            if(t.Length != 4){
                Debug.LogError("SmashLoop의 인자로 4개만 넣으세요!");
                yield return null;
            }
            else{
                _isSmashing = true;

                Vector3 headPos = _bones[0].position;

                // 위로 올라감
                var temp = Vector3.zero;
                yield return StartCoroutine(MoveBonesLoop(t[0], temp));

                // 목표 x 좌표로 이동
                temp = new Vector3(dest.x, 0, 0);
                yield return StartCoroutine(MoveBonesLoop(t[1], temp));

                // 약간 위로 올림
                temp += Vector3.up * 10;
                yield return StartCoroutine(MoveBonesLoop(t[2], temp));

                // 내려찍기
                temp = new Vector3(0, -15, 0);
                yield return StartCoroutine(MoveBonesLoop(t[3], dest));

                MoveBones(dest);
                _isSmashing = false;
                yield return null;
            }

        }

        IEnumerator MoveBonesLoop(float t, Vector3 dest){
            float duration = t;

            while(duration > 0){
                Vector3 headPos = _bones[0].position;
                float ratio = 1f - (duration / t);
                var result = Vector3.Lerp(headPos, dest, ratio);
                MoveBones(result);
                duration -= Time.deltaTime;
                yield return null;
            }
            MoveBones(dest);

            yield return null;
        }

        void MoveBones(Vector3 target){
            Vector3 temp = target;

            var basePos = (_bones[_bones.Count - 1].position);
            for(int i = 0; i < _bones.Count - 1; i++){
                var result = IK.Reach(_bones[i].position, _bones[i + 1].position, temp);
                _bones[i].position = result[0];
                temp = result[1];
            }
            _bones[_bones.Count - 1].position = (temp);

            temp = (basePos);
            for(int i = _bones.Count - 1; i > 0; i--){
                var result = IK.Reach(_bones[i].position, _bones[i - 1].position, temp);
                _bones[i].position = result[0];
                temp = result[1];
            }
            _bones[0].position = (temp);
            DrawLine();
        }
        #endregion

        #endregion
    }
}
