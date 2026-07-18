using System.Reflection;

namespace EasyCore.AspNetCore.Mvc.RemoteServices
{
    /// <summary>
    /// Factory for creating remote API clients whose base address is resolved via Nacos.
    /// </summary>
    public static class RemoteApiNacosClientFactory
    {
        /// <summary>
        /// Creates a typed DispatchProxy client for interface <typeparamref name="T"/>.
        /// </summary>
        public static T Create<T>(
            HttpClient httpClient,
            string? routePrefix = null,
            IRemoteRequestHeaderProvider? headerProvider = null) where T : class
        {
            if (!typeof(T).IsInterface)
                throw new ArgumentException($"{typeof(T).FullName} must be an interface for DispatchProxy.", nameof(T));

            routePrefix = string.IsNullOrWhiteSpace(routePrefix)
                ? RemoteHttpClientProxy.BuildRoutePrefix(typeof(T))
                : routePrefix;

            var proxy = DispatchProxy.Create<T, RemoteHttpClientProxy>();
            ((RemoteHttpClientProxy)(object)proxy).Configure(httpClient, routePrefix!, headerProvider);
            return proxy;
        }

        /// <summary>
        /// Creates a non-generic DispatchProxy client for the specified interface type.
        /// </summary>
        public static object Create(
            HttpClient httpClient,
            IRemoteRequestHeaderProvider? headerProvider,
            Type iface)
        {
            if (!iface.IsInterface)
                throw new ArgumentException($"{iface.FullName} must be an interface for DispatchProxy.", nameof(iface));

            var method = typeof(RemoteApiNacosClientFactory)
                .GetMethod(nameof(Create), new[] { typeof(HttpClient), typeof(string), typeof(IRemoteRequestHeaderProvider) })!
                .MakeGenericMethod(iface);

            return method.Invoke(null, new object?[] { httpClient, null, headerProvider })!;
        }
    }
}
