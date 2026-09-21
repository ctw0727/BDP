using Reflex.Core;

namespace BS.Runtime.Extensions
{
    public static class ContainerExtensions
    {
        public static bool TryResolve<T>(this Container container, out T service)
        {
            if (container.TryGetResolver<T>(out var resolver))
            {
                service = (T)resolver.Resolve(container);
                return true;
            }

            service = default;
            return false;
        }
    }
}