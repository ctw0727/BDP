using System;
using Reflex.Attributes;
using Unity.U2D.Physics;

namespace BS.Physics
{
    public class PhysicsWorldService : IDisposable
    {
        public PhysicsWorld World => _physicsWorld;

        PhysicsWorld _physicsWorld;

        [Inject]
        void Initialize(PhysicsCoreSettings2D settings)
        {
            _physicsWorld = PhysicsWorld.Create(settings.physicsWorldDefinition);
            _physicsWorld.autoContactCallbacks = true;
            _physicsWorld.autoTriggerCallbacks = true;
            PhysicsDrawer.Instance.Initialize(this);
        }

        void IDisposable.Dispose()
        {
            _physicsWorld.Destroy();
        }
    }
}