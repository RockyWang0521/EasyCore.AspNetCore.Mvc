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
        /// The Consul address is read from <c>Consul:ConsulAddress</c> when the client is resolved.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
        public static IServiceCollection AddEasyCoreRemoteApiConsulClients(this IServiceCollection services)
        {
            services.TryAddSingleton<IRemoteRequestHeaderProvider, HttpContextHeaderProvider>();
            services.AddHttpContextAccessor();

            services.TryAddSingleton<IConsulClient>(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var consulAddress = configuration["Consul:ConsulAddress"];
                if (string.IsNullOrWhiteSpace(consulAddress))
                    throw new InvalidOperationException("Consul address is not configured (Consul:ConsulAddress).");

                return new ConsulClient(config => config.Address = new Uri(consulAddress));
            });

            services.TryAddSingleton<ConsulServiceDiscovery>();

            foreach (var iface in RemoteApiRegistrationHelper.FindRemoteInterfaces())
            {
                var attr = iface.GetCustomAttribute<ConsulServiceAttribute>();
                if (attr == null)
                    continue;

                // Provider hosts already have a concrete AppService; do not replace it with an HTTP proxy.
                if (RemoteApiRegistrationHelper.HasLocalImplementation(services, iface))
                    continue;

                var clientName = iface.FullName!;
                var serviceName = attr.ServiceName;

                // HttpClient rejects relative RequestUri when BaseAddress is unset, before handlers run.
                // Use a placeholder host; ConsulResolvingHandler rewrites it to the discovered instance.
                services.AddHttpClient(clientName, client =>
                    {
                        client.BaseAddress = new Uri("http://consul.placeholder/");
                    })
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
