using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EasyCore.AspNetCore.Mvc.RemoteServices
{
    /// <summary>
    /// Extension methods for registering remote API clients against configured static hosts.
    /// </summary>
    public static class RemoteApiClientExtensions
    {
        /// <summary>
        /// Scans loaded assemblies for <see cref="IRemoteAppService"/> interfaces marked with
        /// <see cref="ApiHostAttribute"/> and registers interface-only HTTP proxies.
        /// Interfaces that already have a local concrete implementation are skipped (provider host).
        /// Base URLs are read from <c>RemoteServices:{ConfigKey}</c> when the client is resolved.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
        public static IServiceCollection EasyCoreRemoteApiClients(this IServiceCollection services)
        {
            services.TryAddSingleton<IRemoteRequestHeaderProvider, HttpContextHeaderProvider>();
            services.AddHttpContextAccessor();

            foreach (var iface in RemoteApiRegistrationHelper.FindRemoteInterfaces())
            {
                var attr = iface.GetCustomAttribute<ApiHostAttribute>();
                if (attr == null)
                    continue;

                // Provider hosts already have a concrete AppService; do not replace it with an HTTP proxy.
                if (RemoteApiRegistrationHelper.HasLocalImplementation(services, iface))
                    continue;

                var clientName = iface.FullName!;
                var configKey = attr.ConfigKey;

                services.AddHttpClient(clientName, (sp, client) =>
                {
                    var config = sp.GetRequiredService<IConfiguration>();
                    var baseUrl = config[$"RemoteServices:{configKey}"];
                    if (string.IsNullOrWhiteSpace(baseUrl))
                        throw new InvalidOperationException($"Configure RemoteServices:{configKey} is null or empty.");

                    client.BaseAddress = new Uri(baseUrl);
                });

                RemoteApiRegistrationHelper.ReplaceWithInterfaceProxy(services, iface, sp =>
                {
                    var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient(clientName);
                    var headerProvider = sp.GetRequiredService<IRemoteRequestHeaderProvider>();
                    return RemoteApiHostClientFactory.Create(httpClient, null, headerProvider, iface);
                });
            }

            return services;
        }
    }
}
