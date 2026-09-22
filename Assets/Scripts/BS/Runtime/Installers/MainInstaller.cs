using BS.Camera;
using BS.Physics;
using BS.Runtime.Input;
using Reflex.Core;
using Reflex.Enums;
using Unity.U2D.Physics;
using UnityEngine;

namespace BS.Installers
{
    public class MainInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField] PhysicsCoreSettings2D _physicsCoreSettings;

        void IInstaller.InstallBindings(ContainerBuilder builder)
        {
            builder
                .RegisterType(typeof(PhysicsWorldService),
                    Lifetime.Singleton, Reflex.Enums.Resolution.Eager)
                .RegisterValue(_physicsCoreSettings)
                .RegisterType(typeof(InputService), Lifetime.Singleton, Reflex.Enums.Resolution.Eager)
                .RegisterType(typeof(CameraService), Lifetime.Singleton, Reflex.Enums.Resolution.Eager);
        }
    }
}