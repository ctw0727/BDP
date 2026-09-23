using BS.Physics;
using BS.Player;
using BS.Runtime.Extensions;
using Reflex.Extensions;
using Unity.U2D.Physics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BS.Intro
{
    public class IntroStartArea : MonoBehaviour, PhysicsCallbacks.ITriggerCallback
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
            bodyDef.type = PhysicsBody.BodyType.Static;
            bodyDef.position = transform.position;
            bodyDef.transformWriteMode = PhysicsBody.TransformWriteMode.Off;

            _body = world.CreateBody(bodyDef);
            _body.callbackTarget = this;

            PhysicsUserData userData = _body.userData;
            userData.objectValue = gameObject;
            _body.userData = userData;

            PhysicsShapeDefinition shapeDef = PhysicsShapeDefinition.defaultDefinition;
            shapeDef.contactFilter = new PhysicsShape.ContactFilter(_category, _contact);
            shapeDef.isTrigger = true;
            shapeDef.triggerEvents = true;

            Vector2 scale = transform.lossyScale;
            Vector2 absScale = new Vector2(Mathf.Abs(scale.x), Mathf.Abs(scale.y));
            foreach (Collider2D legacy in GetComponents<Collider2D>())
            {
                Vector2 offset = Vector2.Scale(legacy.offset, scale);
                PhysicsShape shape = default;
                if (legacy is BoxCollider2D box)
                {
                    PolygonGeometry geometry = PolygonGeometry.CreateBox(Vector2.Scale(box.size, absScale), 0f, new PhysicsTransform(offset), false);
                    shape = _body.CreateShape(geometry, shapeDef);
                }
                else if (legacy is CircleCollider2D circle)
                {
                    CircleGeometry geometry = CircleGeometry.Create(circle.radius * Mathf.Max(absScale.x, absScale.y), offset);
                    shape = _body.CreateShape(geometry, shapeDef);
                }

                if (shape.isValid)
                    shape.callbackTarget = this;

                legacy.enabled = false;
            }
        }

        public void OnTriggerBegin2D(PhysicsEvents.TriggerBeginEvent beginEvent)
        {
            PhysicsShape visitor = beginEvent.visitorShape;
            if (!visitor.isValid)
                return;

            GameObject other = visitor.body.userData.objectValue as GameObject;
            if (other != null && other.CompareTag("Player"))
            {
                other.GetComponent<PlayerController>().IsControllable = true;
                SceneManager.LoadScene("BS_Scene");
                Destroy(this.gameObject);
            }
        }

        public void OnTriggerEnd2D(PhysicsEvents.TriggerEndEvent endEvent)
        {

        }
    }
}
