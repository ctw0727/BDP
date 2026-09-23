using System.Collections.Generic;
using BS.BehaviorTrees.Tasks;
using BS.BehaviorTrees.Trees;
using BS.Camera;
using BS.Projectile;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BS.Enemy.Boss
{
    public class BossBehavior : BaseBossBehavior
    {
        public GameObject _bulletPrefab;

        readonly List<Bullet> _bullets = new List<Bullet>();
        int _attackType;

        [Inject] CameraService _cameraService;

        public override void Init()
        {
            base.Init();
            _boss.SetMaxHealth(10000);

            _tree = new BehaviorTreeBuilder(gameObject)
                .Selector()
                    .Condition(() => _boss.IsDead)
                    .Condition(() => _boss.IsInvincible)
                    .Sequence()
                        .Selector("Attack")
                            .Sequence("pattern_0")
                                .Condition(() => _attackType == 0)
                                .Do("Fire", () => { FireRing(25f, 20f, 0f); return TaskStatus.Success; })
                                .WaitTime(0.8f)
                                .Do("Fire", () => { FireRing(25f, 20f, 0f); return TaskStatus.Success; })
                                .WaitTime(0.8f)
                                .Do("Fire", () =>
                                {
                                    FireRing(25f, 20f, 0f);
                                    _attackType = (_attackType + 1) % 5;
                                    return TaskStatus.Success;
                                })
                                .WaitTime(0.8f)
                            .End()
                            .Sequence("pattern_1")
                                .Condition(() => _attackType == 1)
                                .Do("Fire", () =>
                                {
                                    FireTriRing();
                                    _attackType = (_attackType + 1) % 5;
                                    return TaskStatus.Success;
                                })
                                .WaitTime(1.5f)
                            .End()
                            .Sequence("pattern_2")
                                .Condition(() => _attackType == 2)
                                .Do("Fire", () =>
                                {
                                    FireSpiral();
                                    _attackType = (_attackType + 1) % 5;
                                    return TaskStatus.Success;
                                })
                                .WaitTime(3f)
                            .End()
                            .Sequence("pattern_3")
                                .Condition(() => _attackType == 3)
                                .Do("Fire", () => { FireAimedSpread(); return TaskStatus.Success; })
                                .WaitTime(0.8f)
                                .Do("Fire", () => { FireAimedSpread(); return TaskStatus.Success; })
                                .WaitTime(0.8f)
                                .Do("Fire", () => { FireAimedSpread(); return TaskStatus.Success; })
                                .WaitTime(0.8f)
                                .Do("Fire", () =>
                                {
                                    FireAimedSpread();
                                    _attackType = (_attackType + 1) % 5;
                                    return TaskStatus.Success;
                                })
                                .WaitTime(0.8f)
                            .End()
                            .Sequence("pattern_4")
                                .Condition(() => _attackType == 4)
                                .Do("Fire", () => { FireAroundPlayer(); return TaskStatus.Success; })
                                .WaitTime(1.6f)
                                .Do("Fire", () => { FireAroundPlayer(); return TaskStatus.Success; })
                                .WaitTime(1.6f)
                                .Do("Fire", () =>
                                {
                                    FireAroundPlayer();
                                    _attackType = (_attackType + 1) % 5;
                                    return TaskStatus.Success;
                                })
                                .WaitTime(1.6f)
                            .End()
                        .End()
                    .End()
                .Build();
        }

        public override void OnDamaged(float damage)
        {
            if (_boss.IsInvincible)
                return;

            if (_boss.Health > damage)
            {
                _boss.SetHealth(_boss.Health - damage);
                _attackType = 0;
                _eraser?.EraserWave(1.2f, 0.75f);
                _boss.IsInvincible = true;
                Invoke(nameof(EndInvincible), 2f);
                _cameraService?.ShakeCamera(1f, 0.1f);
            }
            else if (_boss.Health != 0f)
            {
                _boss.SetHealth(0f);
                _boss.IsDead = true;
                _eraser?.EraserWave(3f, 0.25f);
                _cameraService?.ShakeCamera(2f, 0.2f);

                SceneManager.LoadScene("Intro");
            }

            OnHit.Invoke();
        }

        void EndInvincible()
        {
            _boss.IsInvincible = false;
        }

        void FireRing(float speed, float step, float offset)
        {
            float spin = Random.Range(0f, step);
            for (float angle = -180f; angle < 180f; angle += step)
            {
                Fire(Origin, Direction(angle + spin + offset), speed, angle + spin + offset);
                Fire(Origin, Direction(angle - step * 0.5f + spin + offset), speed * 0.72f, angle + spin + offset);
            }
        }

        void FireTriRing()
        {
            float spin = Random.Range(0f, 9f);
            for (float angle = 0f; angle < 360f; angle += 10f)
            {
                Fire(Origin, Direction(angle + spin), 25f, angle + spin);
                Fire(Origin, Direction(120f + angle + spin), 25f, 120f + angle + spin);
                Fire(Origin, Direction(240f + angle + spin), 25f, 240f + angle + spin);
            }
        }

        void FireSpiral()
        {
            for (float angle = 0f; angle < 360f; angle += 8f)
            {
                float spin = Random.Range(0f, 50f);
                float speed = 10f - angle / 120f;
                Fire(Origin, Direction(angle + spin), speed, angle + spin);
                Fire(Origin, Direction(120f + angle + spin), speed, angle + spin);
                Fire(Origin, Direction(240f + angle + spin), speed, angle + spin);
            }
        }

        void FireAimedSpread()
        {
            Vector2 player = PlayerPosition;
            float distance = Vector2.Distance(player, Origin);
            float angle = AngleTo(player);
            float speed = 50f - distance;
            Fire(Origin, Direction(angle), speed, angle);
            Fire(Origin, Direction(angle + 7f), speed, angle + 7f);
            Fire(Origin, Direction(angle - 7f), speed, angle - 7f);
        }

        void FireAroundPlayer()
        {
            Vector2 player = PlayerPosition;
            float spin = Random.Range(40f, 50f);
            for (float angle = 0f; angle < 360f; angle += 30f)
            {
                Vector2 spawn = player + Direction(angle + spin) * 7f;
                float shot = angle + spin + 180f;
                Fire(spawn, Direction(shot), 25f, shot);
            }
        }

        void Fire(Vector2 origin, Vector2 direction, float speed, float angle)
        {
            Bullet bullet = Rent();
            if (bullet == null)
                return;

            bullet.Launch(origin, direction, speed, angle);
        }

        Bullet Rent()
        {
            if (_bulletPrefab == null)
                return null;

            for (int i = 0; i < _bullets.Count; i++)
            {
                if (_bullets[i] != null && !_bullets[i].IsLive)
                    return _bullets[i];
            }

            Bullet created = Instantiate(_bulletPrefab, transform).GetComponent<Bullet>();
            if (created == null)
                return null;

            _bullets.Add(created);
            return created;
        }

        Vector2 Origin => transform.position;

        Vector2 PlayerPosition => _player != null ? _player.Position : Origin;

        float AngleTo(Vector2 target)
        {
            Vector2 from = Origin;
            return Mathf.Atan2(target.y - from.y, target.x - from.x) * Mathf.Rad2Deg;
        }

        static Vector2 Direction(float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        }
    }
}
