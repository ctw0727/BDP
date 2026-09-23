using BS.Physics;
using BS.Runtime.Extensions;
using Reflex.Attributes;
using Reflex.Extensions;
using Unity.Collections;
using Unity.U2D.Physics;
using UnityEngine;

/// <summary>
/// 플레이어의 물리 관련 처리를 위한 컴포넌트
/// </summary>
/// 플레이어가 일정 각도 이상은 못 올라가도록 해야겠음
namespace BS.Player
{
    [DefaultExecutionOrder(-50)]
    public class PlayerPhysicController : MonoBehaviour,
        PhysicsCallbacks.IContactCallback,
        PhysicsCallbacks.ITriggerCallback
    {
        [SerializeField] PhysicsMask _category;
        [SerializeField] PhysicsMask _contact;

        [SerializeField]
        private PhysicsMask _groundLayers = 0;
        [SerializeField]
        Vector2 _boxSize = new Vector2(1.07f, 0.7f);
        public float _groundRadius = .2f;

        /* 물리 관련 bool */
        [SerializeField]
        protected bool _isGrounded = false;
        public bool _isFalling = false;
        public bool _onAir = false;
        public bool _isMoving = false;

        public Vector2 BoxSize => _boxSize;
        public float RollRadius => Mathf.Max(0.01f, _boxSize.y * 0.5f);
        public Vector2 Position => _body.isValid ? _body.position : (Vector2)transform.position;

        public Vector2 LinearVelocity
        {
            get => _body.isValid ? _body.linearVelocity : Vector2.zero;
            set
            {
                if (_body.isValid)
                    _body.linearVelocity = value;
            }
        }

        public float LinearDamping
        {
            get => _body.isValid ? _body.linearDamping : 0f;
            set
            {
                if (_body.isValid)
                    _body.linearDamping = value;
            }
        }

        public float AngularDamping
        {
            get => _body.isValid ? _body.angularDamping : 0f;
            set
            {
                if (_body.isValid)
                    _body.angularDamping = value;
            }
        }

        public float GravityScale
        {
            get => _body.isValid ? _body.gravityScale : 0f;
            set
            {
                if (_body.isValid)
                    _body.gravityScale = value;
            }
        }

        PlayerController _player;
        PhysicsBody _body;
        PhysicsShape _shape;
        bool _frozen;

        public void Freeze()
        {
            _frozen = true;
            if (_body.isValid)
                _body.constraints = PhysicsBody.BodyConstraints.All;
        }

        public void UnFreeze()
        {
            _frozen = false;
            if (_body.isValid)
                _body.constraints = PhysicsBody.BodyConstraints.Rotation;
        }

        public void AddForce(Vector2 dir)
        {
            if (_body.isValid)
                _body.ApplyForceToCenter(dir, true);
        }

        public void ApplyImpulse(Vector2 impulse)
        {
            if (_body.isValid)
                _body.ApplyLinearImpulseToCenter(impulse, true);
        }

        public void SetSurface(float friction, float bounciness)
        {
            if (!_shape.isValid)
                return;

            PhysicsShape.SurfaceMaterial material = _shape.surfaceMaterial;
            material.friction = friction;
            material.bounciness = bounciness;
            _shape.surfaceMaterial = material;
        }

        public void Init(PlayerController player)
        {
            _player = player;
        }

        [Inject]
        void Initialize(PhysicsWorldService physicsWorldService)
        {
            CreateBody(physicsWorldService.World);
        }

        void Start()
        {
            if (_body.isValid)
                return;

            var container = gameObject.scene.GetSceneContainer();
            if (container != null && container.TryResolve<PhysicsWorldService>(out var physicsWorldService))
                CreateBody(physicsWorldService.World);
        }

        void CreateBody(PhysicsWorld world)
        {
            if (_body.isValid || !world.isValid)
                return;

            PhysicsBodyDefinition bodyDef = PhysicsBodyDefinition.defaultDefinition;
            bodyDef.type = PhysicsBody.BodyType.Dynamic;
            bodyDef.position = transform.position;
            bodyDef.constraints = _frozen ? PhysicsBody.BodyConstraints.All : PhysicsBody.BodyConstraints.Rotation;
            bodyDef.gravityScale = 9.8f;
            bodyDef.linearDamping = 0.1f;
            bodyDef.angularDamping = 0.2f;
            bodyDef.fastCollisionsAllowed = true;
            bodyDef.transformWriteMode = PhysicsBody.TransformWriteMode.Off;

            _body = world.CreateBody(bodyDef);
            _body.callbackTarget = this;

            PhysicsUserData userData = _body.userData;
            userData.objectValue = gameObject;
            _body.userData = userData;

            PhysicsShapeDefinition shapeDef = PhysicsShapeDefinition.defaultDefinition;
            shapeDef.contactFilter = _category.bitMask == 0 && _contact.bitMask == 0
                ? PhysicsShape.ContactFilter.defaultFilter
                : new PhysicsShape.ContactFilter(_category, _contact);
            shapeDef.contactEvents = true;
            shapeDef.triggerEvents = true;
            shapeDef.density = 1f;

            PhysicsShape.SurfaceMaterial surface = shapeDef.surfaceMaterial;
            surface.friction = 0.4f;
            surface.bounciness = 0f;
            shapeDef.surfaceMaterial = surface;

            PolygonGeometry box = PolygonGeometry.CreateBox(_boxSize, 0f, false);
            _shape = _body.CreateShape(box, shapeDef);
            _shape.callbackTarget = this;
        }

        protected void CheckGround()
        {
            bool wasGrounded = _isGrounded;
            _isGrounded = false;

            if (_body.isValid && _player != null)
            {
                Vector2 origin = _body.position + Vector2.down * _groundRadius;
                CircleGeometry circle = new CircleGeometry
                {
                    center = origin,
                    radius = _groundRadius
                };

                PhysicsQuery.QueryFilter query = _category.bitMask == 0 && _groundLayers.bitMask == 0
                    ? PhysicsQuery.QueryFilter.defaultFilter
                    : new PhysicsQuery.QueryFilter(_category, _groundLayers);

                NativeArray<PhysicsQuery.WorldOverlapResult> overlaps = _body.world.OverlapGeometry(
                    circle,
                    query,
                    Allocator.Temp);

                if (overlaps.Length > 0)
                {
                    try
                    {
                        for (int i = 0; i < overlaps.Length; i++)
                        {
                            PhysicsShape shape = overlaps[i].shape;
                            if (!shape.isValid || shape.body == _body)
                                continue;

                            GameObject other = shape.body.userData.objectValue as GameObject;
                            if (other == null || other == gameObject)
                                continue;

                            _isGrounded = true;
                            _player.OnSurfaceContact(other);
                            break;
                        }
                    }
                    finally
                    {
                        overlaps.Dispose();
                    }
                }
            }

            _onAir = false;
            _isFalling = false;
            if (wasGrounded == false && _isGrounded == false)
            {
                _onAir = true;

                if (LinearVelocity.y < 0)
                    _isFalling = true;
            }
        }

        void Update()
        {
            if (!_body.isValid)
                return;

            Vector2 position = _body.position;
            transform.SetPositionAndRotation(
                new Vector3(position.x, position.y, transform.position.z),
                Quaternion.identity);
        }

        void FixedUpdate()
        {
            CheckGround();
        }

        void OnDestroy()
        {
            if (_body.isValid)
                _body.Destroy();
        }

        public void OnContactBegin2D(PhysicsEvents.ContactBeginEvent beginEvent)
        {
            GameObject other = OtherObject(beginEvent.shapeA, beginEvent.shapeB);
            if (other != null)
                _player?.OnPhysicsContact(other);
        }

        public void OnContactEnd2D(PhysicsEvents.ContactEndEvent endEvent)
        {
        }

        public void OnTriggerBegin2D(PhysicsEvents.TriggerBeginEvent beginEvent)
        {
        }

        public void OnTriggerEnd2D(PhysicsEvents.TriggerEndEvent endEvent)
        {
        }

        GameObject OtherObject(PhysicsShape shapeA, PhysicsShape shapeB)
        {
            PhysicsShape other = shapeA.body == _body ? shapeB : shapeA;
            if (!other.isValid)
                return null;

            return other.body.userData.objectValue as GameObject;
        }

        void OnDrawGizmos()
        {
            Vector3 origin = transform.position;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(origin, new Vector3(_boxSize.x, _boxSize.y, 0f));

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(origin + Vector3.down * _groundRadius, _groundRadius);
        }
    }
}
