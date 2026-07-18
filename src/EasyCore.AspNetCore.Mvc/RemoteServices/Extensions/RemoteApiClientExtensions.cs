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
        /// Local host implementations are kept when remote configuration is missing.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when a remote interface requires a missing <c>RemoteServices:{ConfigKey}</c> value.
        /// </exception>
        public static IServiceCollection EasyCoreRemoteApiClients(this IServiceCollection services)
        {
            services.TryAddSingleton<IRemoteRequestHeaderProvider, HttpContextHeaderProvider>();
            services.AddHttpContextAccessor();

            var configuration = RemoteApiRegistrationHelper.TryGetConfiguration(services);

            foreach (var iface in RemoteApiRegistrationHelper.FindRemoteInterfaces())
            {
                var attr = iface.GetCustomAttribute<ApiHostAttribute>();
                if (attr == null)
                    continue;

                var hasConfig = RemoteApiRegistrationHelper.HasApiHostConfig(configuration, attr.ConfigKey);
                if (!RemoteApiRegistrationHelper.ShouldRegisterRemoteProxy(services, iface, requireRemoteConfig: true, hasConfig))
                    continue;

                if (!hasConfig)
                    throw new InvalidOperationException(
                        $"Configure RemoteServices:{attr.ConfigKey} for remote interface {iface.Name}.");

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
