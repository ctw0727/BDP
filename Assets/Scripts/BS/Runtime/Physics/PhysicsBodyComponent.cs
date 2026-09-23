using System.Collections.Generic;
using System.Linq;
using Reflex.Attributes;
using SaintsField;
using Unity.U2D.Physics;
using UnityEngine;

namespace BS.Physics
{
    /// <summary>
    /// Physics Core 2D 래퍼
    /// </summary>
    public class PhysicsBodyComponent : MonoBehaviour
    {
        [SerializeField] PhysicsShapeDefinition _shapeDef;
        [SerializeField] List<Vector2> _shapePoints;

        PhysicsBody _body;

        public PhysicsBody Body => _body;

        [Inject]
        void Initialize(PhysicsWorldService physicsWorldService)
        {
            PhysicsBody body = physicsWorldService.World.CreateBody();
            body.transformWriteMode = PhysicsBody.TransformWriteMode.Off;

            PhysicsUserData userData = body.userData;
            userData.objectValue = gameObject;
            body.userData = userData;

            PhysicsShapeDefinition shapeDef = _shapeDef;
            if (shapeDef.contactFilter.categories.bitMask == 0)
                shapeDef.contactFilter = PhysicsShape.ContactFilter.defaultFilter;

            body.CreateShapeBatch(PolygonGeometry.CreatePolygons(_shapePoints.ToArray(), new PhysicsTransform(this.transform.position, PhysicsRotate.identity)), shapeDef);
            _body = body;
        }

        void OnDestroy()
        {
            if (_body.isValid)
                _body.Destroy();
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (_shapePoints == null || _shapePoints.Count < 2)
            {
                return;
            }

            Gizmos.color = _shapeDef.surfaceMaterial.customColor;
            Gizmos.DrawLineStrip(_shapePoints.Select(p => ((Vector3)p + this.transform.position)).ToArray(), true);
        }

        void OnValidate()
        {
            if (_shapePoints != null && _shapePoints.Count > 0)
            {
                return;
            }

            SpriteRenderer renderer = GetComponent<SpriteRenderer>();

            if (renderer != null && renderer.sprite != null)
            {
                renderer.sprite.GetPhysicsShape(0, _shapePoints);
            }
        }
#endif
    }
}
