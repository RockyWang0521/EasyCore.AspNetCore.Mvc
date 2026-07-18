using System.Reflection;
using EasyCore.AspNetCore.Mvc.RemoteServices.K8sOptions;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EasyCore.AspNetCore.Mvc.RemoteServices
{
    /// <summary>
    /// Extension methods for registering remote API clients that use Kubernetes DNS host names.
    /// </summary>
    public static class RemoteApiK8sClientExtensions
    {
        /// <summary>
        /// Registers interface-only proxies for contracts marked with <see cref="K8sDnsAttribute"/>.
        /// Skips interfaces that already have a local implementation.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <param name="configure">Configures Kubernetes namespace and cluster domain options.</param>
        /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="configure"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when required K8s options are missing.</exception>
        public static IServiceCollection EasyCoreRemoteApiK8sClients(
            this IServiceCollection services,
            Action<K8sOption> configure)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            var option = new K8sOption();
            configure(option);

            if (string.IsNullOrWhiteSpace(option.K8sNamespace) || string.IsNullOrWhiteSpace(option.K8sClusterDomain))
                throw new InvalidOperationException("K8sOption is not configured.");

            // Normalize: users may pass either "cluster.local" or the older "svc.cluster.local".
            var clusterDomain = option.K8sClusterDomain.Trim().Trim('.');
            if (clusterDomain.StartsWith("svc.", StringComparison.OrdinalIgnoreCase))
                clusterDomain = clusterDomain["svc.".Length..];

            services.AddOptions();
            services.Configure(configure);
            services.TryAddSingleton<IRemoteRequestHeaderProvider, HttpContextHeaderProvider>();
            services.AddHttpContextAccessor();

            foreach (var iface in RemoteApiRegistrationHelper.FindRemoteInterfaces())
            {
                var attr = iface.GetCustomAttribute<K8sDnsAttribute>();
                if (attr == null)
                    continue;

                // Provider hosts already have a concrete AppService; do not replace it with an HTTP proxy.
                if (RemoteApiRegistrationHelper.HasLocalImplementation(services, iface))
                    continue;

                var clientName = iface.FullName!;
                var baseAddress = BuildK8sBaseAddress(attr.ServiceName, option.K8sNamespace, clusterDomain, attr.Port);

                services.AddHttpClient(clientName, client => client.BaseAddress = baseAddress);

                RemoteApiRegistrationHelper.ReplaceWithInterfaceProxy(services, iface, sp =>
                {
                    var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient(clientName);
                    var headerProvider = sp.GetRequiredService<IRemoteRequestHeaderProvider>();
                    return RemoteApiK8sClientFactory.Create(httpClient, headerProvider, iface);
                });
            }

            return services;
        }

        /// <summary>
        /// Builds a Kubernetes service DNS base address ending with a trailing slash.
        /// Format: <c>http://{service}.{namespace}.svc.{clusterDomain}[:port]/</c>
        /// </summary>
        internal static Uri BuildK8sBaseAddress(string serviceName, string @namespace, string clusterDomain, int? port)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
                throw new ArgumentException("Kubernetes service name is required.", nameof(serviceName));
            if (string.IsNullOrWhiteSpace(@namespace))
                throw new ArgumentException("Kubernetes namespace is required.", nameof(@namespace));
            if (string.IsNullOrWhiteSpace(clusterDomain))
                throw new ArgumentException("Kubernetes cluster domain is required.", nameof(clusterDomain));

            var host = $"{serviceName.Trim()}.{@namespace.Trim()}.svc.{clusterDomain.Trim().Trim('.')}";
            var authority = port is null or 80 ? host : $"{host}:{port.Value}";
            return new Uri($"http://{authority}/");
        }
    }
}
