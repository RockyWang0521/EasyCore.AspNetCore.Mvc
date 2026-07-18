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

            services.AddOptions();
            services.Configure(configure);
            services.TryAddSingleton<IRemoteRequestHeaderProvider, HttpContextHeaderProvider>();
            services.AddHttpContextAccessor();

            foreach (var iface in RemoteApiRegistrationHelper.FindRemoteInterfaces())
            {
                var attr = iface.GetCustomAttribute<K8sDnsAttribute>();
                if (attr == null)
                    continue;

                if (!RemoteApiRegistrationHelper.ShouldRegisterRemoteProxy(
                        services, iface, requireRemoteConfig: false, hasRemoteConfig: false))
                    continue;

                var clientName = iface.FullName!;
                var fullDns = $"http://{attr.ServiceName}.{option.K8sNamespace}.{option.K8sClusterDomain}";

                services.AddHttpClient(clientName, client => client.BaseAddress = new Uri(fullDns));

                RemoteApiRegistrationHelper.ReplaceWithInterfaceProxy(services, iface, sp =>
                {
                    var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient(clientName);
                    var headerProvider = sp.GetRequiredService<IRemoteRequestHeaderProvider>();
                    return RemoteApiK8sClientFactory.Create(httpClient, headerProvider, iface);
                });
            }

            return services;
        }
    }
}
