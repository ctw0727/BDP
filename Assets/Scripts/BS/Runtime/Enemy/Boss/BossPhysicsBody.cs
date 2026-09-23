using BS.Physics;
using BS.Runtime.Extensions;
using Reflex.Extensions;
using Unity.U2D.Physics;
using UnityEngine;

namespace BS.Enemy.Boss
{
    /// <summary>
    /// 보스의 비트리거 Collider2D 모양을 따라 Physics Core 월드에 Kinematic 바디를 만들고 Transform을 따라가게 합니다.
    /// </summary>
    public class BossPhysicsBody : MonoBehaviour
    {
        [SerializeField] PhysicsMask _category = 1UL;
        [SerializeField] PhysicsMask _contact = 1UL << 8;

        PhysicsBody _body;

        void Start()
        {
            var container = gameObject.scene.GetSceneContainer();
            if (container != null && container.TryResolve<PhysicsWorldService>(out PhysicsWorldService service))
                CreateBody(service.World);
        }

        void CreateBody(PhysicsWorld world)
        {
            if (_body.isValid || !world.isValid)
                return;

            PhysicsBodyDefinition bodyDef = PhysicsBodyDefinition.defaultDefinition;
            bodyDef.type = PhysicsBody.BodyType.Kinematic;
            bodyDef.position = transform.position;
            bodyDef.transformWriteMode = PhysicsBody.TransformWriteMode.Off;

            _body = world.CreateBody(bodyDef);

            PhysicsUserData userData = _body.userData;
            userData.objectValue = gameObject;
            _body.userData = userData;

            PhysicsShapeDefinition shapeDef = PhysicsShapeDefinition.defaultDefinition;
            shapeDef.contactFilter = new PhysicsShape.ContactFilter(_category, _contact);
            shapeDef.contactEvents = true;

            Vector2 scale = transform.lossyScale;
            foreach (Collider2D legacy in GetComponents<Collider2D>())
            {
                if (!legacy.isTrigger)
                    CreateShape(legacy, scale, shapeDef);

                legacy.enabled = false;
            }
        }

        void CreateShape(Collider2D legacy, Vector2 scale, PhysicsShapeDefinition shapeDef)
        {
            Vector2 offset = Vector2.Scale(legacy.offset, scale);

            if (legacy is CircleCollider2D circle)
            {
                float radius = circle.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y));
                _body.CreateShape(CircleGeometry.Create(radius, offset), shapeDef);
            }
            else if (legacy is BoxCollider2D box)
            {
                Vector2 size = Vector2.Scale(box.size, new Vector2(Mathf.Abs(scale.x), Mathf.Abs(scale.y)));
                _body.CreateShape(PolygonGeometry.CreateBox(size, 0f, new PhysicsTransform(offset), false), shapeDef);
            }
        }

        void FixedUpdate()
        {
            if (_body.isValid)
                _body.SetTransformTarget(new PhysicsTransform(transform.position), Time.fixedDeltaTime);
        }

        void OnDestroy()
        {
            if (_body.isValid)
                _body.Destroy();
        }
    }
}
