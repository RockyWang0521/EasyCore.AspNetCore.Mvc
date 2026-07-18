using System.Reflection;
using Consul;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EasyCore.AspNetCore.Mvc.RemoteServices
{
    /// <summary>
    /// Extension methods for registering remote API clients that resolve hosts via Consul.
    /// </summary>
    public static class RemoteApiConsulClientExtensions
    {
        /// <summary>
        /// Registers Consul discovery and interface-only proxies for contracts marked with
        /// <see cref="ConsulServiceAttribute"/>. Skips interfaces that already have a local implementation.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when <c>Consul:ConsulAddress</c> is not configured.</exception>
        public static IServiceCollection EasyCoreRemoteApiConsulClients(this IServiceCollection services)
        {
            services.TryAddSingleton<IRemoteRequestHeaderProvider, HttpContextHeaderProvider>();
            services.AddHttpContextAccessor();

            var configuration = RemoteApiRegistrationHelper.TryGetConfiguration(services);
            var consulAddress = configuration?["Consul:ConsulAddress"];
            if (string.IsNullOrWhiteSpace(consulAddress))
                throw new ArgumentException("Consul address is not configured (Consul:ConsulAddress).");

            services.TryAddSingleton<IConsulClient>(_ => new ConsulClient(config =>
            {
                config.Address = new Uri(consulAddress);
            }));

            services.TryAddSingleton<ConsulServiceDiscovery>();

            foreach (var iface in RemoteApiRegistrationHelper.FindRemoteInterfaces())
            {
                var attr = iface.GetCustomAttribute<ConsulServiceAttribute>();
                if (attr == null)
                    continue;

                // Host apps that already registered a concrete implementation keep the local service.
                if (!RemoteApiRegistrationHelper.ShouldRegisterRemoteProxy(
                        services, iface, requireRemoteConfig: false, hasRemoteConfig: false))
                    continue;

                var clientName = iface.FullName!;
                var serviceName = attr.ServiceName;

                services.AddHttpClient(clientName)
                    .AddHttpMessageHandler(sp =>
                        new ConsulResolvingHandler(sp.GetRequiredService<ConsulServiceDiscovery>(), serviceName));

                RemoteApiRegistrationHelper.ReplaceWithInterfaceProxy(services, iface, sp =>
                {
                    var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient(clientName);
                    var headerProvider = sp.GetRequiredService<IRemoteRequestHeaderProvider>();
                    return RemoteApiConsulClientFactory.Create(httpClient, headerProvider, iface);
                });
            }

            return services;
        }
    }
}
