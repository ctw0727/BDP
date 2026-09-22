using System.Collections.Generic;
using Reflex.Attributes;
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
            var body = physicsWorldService.World.CreateBody();
            body.CreateShapeBatch(PolygonGeometry.CreatePolygons(_shapePoints.ToArray(), new PhysicsTransform(this.transform.position, PhysicsRotate.identity)), _shapeDef);
        }

#if UNITY_EDITOR
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