using System.Collections.Generic;
using BS.Physics;
using BS.Player;
using BS.Render;
using BS.Runtime.Extensions;
using Reflex.Extensions;
using Unity.U2D.Physics;
using UnityEngine;

namespace BS.Projectile
{
    public class Bullet : MonoBehaviour, PhysicsCallbacks.ITriggerCallback
    {
        public static readonly List<Bullet> Live = new List<Bullet>();

        [SerializeField] Sprite _sprite;
        [SerializeField] Material _material;
        [SerializeField] float _radius = 0.2f;
        [SerializeField] PhysicsMask _category = 1UL << 10;
        [SerializeField] PhysicsMask _contact = 1UL << 8;

        readonly RenderController _render = new RenderController();
        PhysicsWorld _world;
        PhysicsBody _body;
        Vector2 _scale = Vector2.one;
        Color _color = Color.white;
        float _angle;
        bool _live;

        public bool IsLive => _live;
        public Vector2 Position => _body.isValid ? _body.position : (Vector2)transform.position;

        void Awake()
        {
            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                if (_sprite == null)
                    _sprite = renderer.sprite;
                if (_material == null)
                    _material = renderer.sharedMaterial;
                _color = renderer.color;
                renderer.enabled = false;
            }

            Rigidbody2D rigidbody = GetComponent<Rigidbody2D>();
            if (rigidbody != null)
                rigidbody.simulated = false;

            _scale = transform.localScale;
            Collider2D collider = GetComponent<Collider2D>();
            if (collider != null)
                collider.enabled = false;

            if (_material != null)
            {
                _render.SetRender(new RenderParams(_material)
                {
                    sortingOrder = 2,
                    renderingLayerMask = 1u
                });
            }
        }

        public void Launch(Vector2 position, Vector2 direction, float speed, float angleDegrees)
        {
            EnsureBody();
            if (!_body.isValid)
                return;

            _live = true;
            _angle = angleDegrees;
            _body.enabled = true;
            _body.position = position;
            _body.linearVelocity = direction.sqrMagnitude > 0.0001f
                ? direction.normalized * speed
                : Vector2.zero;

            if (!Live.Contains(this))
                Live.Add(this);
        }

        public void Disable()
        {
            _live = false;
            if (_body.isValid)
            {
                _body.linearVelocity = Vector2.zero;
                _body.enabled = false;
            }

            Live.Remove(this);
        }

        public static void Erase(Vector2 center, float radius)
        {
            float radiusSqr = radius * radius;
            for (int i = Live.Count - 1; i >= 0; i--)
            {
                Bullet bullet = Live[i];
                if ((bullet.Position - center).sqrMagnitude <= radiusSqr)
                    bullet.Disable();
            }
        }

        void Update()
        {
            if (!_live || !_body.isValid || _sprite == null || _material == null)
                return;

            Vector2 position = _body.position;
            if (IsOutsideView(position))
            {
                Disable();
                return;
            }

            _render.SetSprite(_sprite)
                .SetColor(_color)
                .SetMatrix(Matrix4x4.TRS(position, Quaternion.Euler(0f, 0f, _angle), _scale))
                .Render();
        }

        void OnDestroy()
        {
            Disable();
            if (_body.isValid)
                _body.Destroy();
        }

        public void OnTriggerBegin2D(PhysicsEvents.TriggerBeginEvent beginEvent)
        {
            if (!_live)
                return;

            GameObject other = OtherObject(beginEvent.triggerShape, beginEvent.visitorShape);
            PlayerController player = other != null ? other.GetComponent<PlayerController>() : null;
            if (player == null)
                return;

            player.OnBulletHit(Position);
            Disable();
        }

        public void OnTriggerEnd2D(PhysicsEvents.TriggerEndEvent endEvent)
        {
        }

        void EnsureBody()
        {
            if (_body.isValid)
                return;

            if (!_world.isValid)
            {
                var container = gameObject.scene.GetSceneContainer();
                if (container == null || !container.TryResolve<PhysicsWorldService>(out PhysicsWorldService service))
                    return;

                _world = service.World;
            }

            if (!_world.isValid)
                return;

            PhysicsBodyDefinition bodyDef = PhysicsBodyDefinition.defaultDefinition;
            bodyDef.type = PhysicsBody.BodyType.Dynamic;
            bodyDef.position = transform.position;
            bodyDef.gravityScale = 0f;
            bodyDef.linearDamping = 0f;
            bodyDef.angularDamping = 0f;
            bodyDef.fastCollisionsAllowed = true;
            bodyDef.constraints = PhysicsBody.BodyConstraints.Rotation;
            bodyDef.transformWriteMode = PhysicsBody.TransformWriteMode.Off;

            _body = _world.CreateBody(bodyDef);
            _body.callbackTarget = this;

            PhysicsUserData userData = _body.userData;
            userData.objectValue = gameObject;
            _body.userData = userData;

            PhysicsShapeDefinition shapeDef = PhysicsShapeDefinition.defaultDefinition;
            shapeDef.contactFilter = _category.bitMask == 0 && _contact.bitMask == 0
                ? PhysicsShape.ContactFilter.defaultFilter
                : new PhysicsShape.ContactFilter(_category, _contact);
            shapeDef.isTrigger = true;
            shapeDef.triggerEvents = true;

            CircleGeometry circle = new CircleGeometry
            {
                center = Vector2.zero,
                radius = Mathf.Max(0.05f, _radius)
            };
            PhysicsShape shape = _body.CreateShape(circle, shapeDef);
            shape.callbackTarget = this;
        }

        GameObject OtherObject(PhysicsShape shapeA, PhysicsShape shapeB)
        {
            PhysicsShape other = shapeA.body == _body ? shapeB : shapeA;
            if (!other.isValid)
                return null;

            return other.body.userData.objectValue as GameObject;
        }

        static bool IsOutsideView(Vector2 position)
        {
            UnityEngine.Camera camera = UnityEngine.Camera.main;
            if (camera == null)
                return false;

            Vector3 screen = camera.WorldToScreenPoint(position);
            const float margin = 64f;
            return screen.z < 0f
                || screen.x < -margin
                || screen.y < -margin
                || screen.x > Screen.width + margin
                || screen.y > Screen.height + margin;
        }
    }
}
