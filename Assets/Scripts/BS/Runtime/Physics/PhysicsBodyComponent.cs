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
            SpriteRenderer renderer = GetComponent<SpriteRenderer>();

            if (renderer != null && renderer.sprite != null)
            {
                renderer.sprite.GetPhysicsShape(0, _shapePoints);
            }
        }
#endif
    }
}