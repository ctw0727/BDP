using Unity.U2D.Physics;
using UnityEngine;

namespace BS.Physics
{
    /// <summary>
    /// 물리 디버깅 도구
    /// 싱글턴, PhysicsWorld의 Draw API 메소드를 활용하여 그림
    /// </summary>
    public class PhysicsDrawer
    {
        public static PhysicsDrawer Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PhysicsDrawer();
                }

                return _instance;
            }
        }

        bool IsValid => _worldService != null && _worldService.World.isValid;

        static PhysicsDrawer _instance;
        PhysicsWorldService _worldService;

        internal PhysicsDrawer()
        {
            _instance = this;
        }

        internal void Initialize(PhysicsWorldService worldService)
        {
            _worldService = worldService;
        }

        public static void DrawCircle(Vector2 pos, float radius, Color color, float lifetime = 0f)
        {
            if (!_instance.IsValid)
            {
                throw new System.Exception("PhysicsDrawer is not initialized with PhysicsWorldService");
            }

            Instance._worldService.World.DrawCircle(pos, radius, color, lifetime);
        }

        public static void DrawBox(Vector2 center, float size, Color color, float lifetime = 0f)
        {
            if (!_instance.IsValid)
            {
                throw new System.Exception("PhysicsDrawer is not initialized with PhysicsWorldService");
            }

            Vector2 lowLeft = center - Vector2.one * size * 0.5f;
            Vector2 highRight = center + Vector2.one * size * 0.5f;
            Instance._worldService.World.DrawAABB(new PhysicsAABB(lowLeft, highRight), color, lifetime);
        }

        public static void DrawBox(Vector2 center, Vector2 size, Color color, float lifetime = 0f)
        {
            if (!_instance.IsValid)
            {
                throw new System.Exception("PhysicsDrawer is not initialized with PhysicsWorldService");
            }

            Vector2 lowLeft = center - size * 0.5f;
            Vector2 highRight = center + size * 0.5f;

            Instance._worldService.World.DrawAABB(new PhysicsAABB(lowLeft, highRight), color, lifetime);
        }

        public static void DrawLine(Vector2 from, Vector2 to, Color color, float lifetime = 0f)
        {
            if (!_instance.IsValid)
            {
                throw new System.Exception("PhysicsDrawer is not initialized with PhysicsWorldService");
            }

            Instance._worldService.World.DrawLine(from, to, color, lifetime);
        }
    }
}