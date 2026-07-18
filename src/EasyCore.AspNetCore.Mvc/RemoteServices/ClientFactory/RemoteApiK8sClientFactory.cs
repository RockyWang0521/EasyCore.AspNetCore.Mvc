using System.Reflection;

namespace EasyCore.AspNetCore.Mvc.RemoteServices
{
    /// <summary>
    /// Factory for creating remote API clients against Kubernetes DNS base addresses.
    /// </summary>
    public static class RemoteApiK8sClientFactory
    {
        /// <summary>
        /// Creates a typed DispatchProxy client for interface <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The remote contract interface.</typeparam>
        /// <param name="httpClient">The HTTP client with a Kubernetes DNS base address.</param>
        /// <param name="headerProvider">Optional provider of headers to forward.</param>
        /// <returns>A proxy implementing <typeparamref name="T"/>.</returns>
        public static T Create<T>(
            HttpClient httpClient,
            IRemoteRequestHeaderProvider? headerProvider = null) where T : class
        {
            if (!typeof(T).IsInterface)
                throw new ArgumentException($"{typeof(T).FullName} must be an interface for DispatchProxy.", nameof(T));

            var routePrefix = RemoteHttpClientProxy.BuildRoutePrefix(typeof(T));
            var proxy = DispatchProxy.Create<T, RemoteHttpClientProxy>();
            ((RemoteHttpClientProxy)(object)proxy).Configure(httpClient, routePrefix, headerProvider);
            return proxy;
        }

        /// <summary>
        /// Creates a non-generic DispatchProxy client for the specified interface type.
        /// </summary>
        /// <param name="httpClient">The HTTP client with a Kubernetes DNS base address.</param>
        /// <param name="headerProvider">Optional provider of headers to forward.</param>
        /// <param name="iface">The remote contract interface type.</param>
        /// <returns>A proxy instance implementing <paramref name="iface"/>.</returns>
        public static object Create(
            HttpClient httpClient,
            IRemoteRequestHeaderProvider? headerProvider,
            Type iface)
        {
            if (!iface.IsInterface)
                throw new ArgumentException($"{iface.FullName} must be an interface for DispatchProxy.", nameof(iface));

            var method = typeof(RemoteApiK8sClientFactory)
                .GetMethod(nameof(Create), new[] { typeof(HttpClient), typeof(IRemoteRequestHeaderProvider) })!
                .MakeGenericMethod(iface);

            return method.Invoke(null, new object?[] { httpClient, headerProvider })!;
        }
    }
}
