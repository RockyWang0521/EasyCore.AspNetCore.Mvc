using System.Reflection;
using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EasyCore.AspNetCore.Mvc.RemoteServices
{
    /// <summary>
    /// Optionally wraps a remote <see cref="DispatchProxy"/> with Castle interceptors
    /// registered as <see cref="IAsyncInterceptor"/> (EasyCore.Invocation / Polly / Redis.Service).
    /// When no interceptors are registered, the DispatchProxy is returned unchanged.
    /// </summary>
    public static class RemoteInterceptorComposer
    {
        /// <summary>
        /// Ensures <see cref="ProxyGenerator"/> is available for optional interceptor stacking.
        /// </summary>
        public static void RegisterCore(IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);
            services.TryAddSingleton<ProxyGenerator>();
        }

        /// <summary>
        /// Wraps <paramref name="dispatchProxy"/> with stacked <see cref="IAsyncInterceptor"/> when present.
        /// </summary>
        /// <param name="iface">The remote contract interface.</param>
        /// <param name="dispatchProxy">The <see cref="DispatchProxy"/> instance implementing <paramref name="iface"/>.</param>
        /// <param name="services">The service provider used to resolve interceptors.</param>
        /// <returns>The original proxy, or a Castle interface proxy when interceptors exist.</returns>
        public static object Wrap(Type iface, object dispatchProxy, IServiceProvider services)
        {
            ArgumentNullException.ThrowIfNull(iface);
            ArgumentNullException.ThrowIfNull(dispatchProxy);
            ArgumentNullException.ThrowIfNull(services);

            if (!iface.IsInterface)
                throw new ArgumentException($"{iface.FullName} must be an interface.", nameof(iface));

            var interceptors = OrderInterceptors(services.GetServices<IAsyncInterceptor>());
            if (interceptors.Length == 0)
                return dispatchProxy;

            var generator = services.GetService<ProxyGenerator>() ?? new ProxyGenerator();
            return generator.CreateInterfaceProxyWithTarget(iface, dispatchProxy, interceptors);
        }

        /// <summary>
        /// Orders interceptors by a public <c>Order</c> property when present (lower = outer).
        /// </summary>
        internal static IAsyncInterceptor[] OrderInterceptors(IEnumerable<IAsyncInterceptor> interceptors)
            => interceptors
                .OrderBy(GetInterceptorOrder)
                .ThenBy(i => i.GetType().FullName, StringComparer.Ordinal)
                .ToArray();

        private static int GetInterceptorOrder(IAsyncInterceptor interceptor)
        {
            var prop = interceptor.GetType().GetProperty(
                "Order",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop?.PropertyType == typeof(int) && prop.GetValue(interceptor) is int order)
                return order;

            return 0;
        }
    }
}
