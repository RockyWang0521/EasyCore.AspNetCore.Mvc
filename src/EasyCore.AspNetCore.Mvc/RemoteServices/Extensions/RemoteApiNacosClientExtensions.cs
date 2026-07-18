using System.Reflection;
using EasyCore.AspNetCore.Mvc.RemoteServices.NacosOptions;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EasyCore.AspNetCore.Mvc.RemoteServices
{
    /// <summary>
    /// Extension methods for registering remote API clients that resolve hosts via Nacos.
    /// </summary>
    public static class RemoteApiNacosClientExtensions
    {
        /// <summary>
        /// Registers Nacos discovery and interface-only proxies for contracts marked with
        /// <see cref="NacosServiceAttribute"/>. Skips interfaces that already have a local implementation.
        /// Reads <c>Nacos:*</c> from configuration when <paramref name="configure"/> is omitted.
        /// </summary>
        public static IServiceCollection EasyCoreRemoteApiNacosClients(
            this IServiceCollection services,
            Action<NacosOption>? configure = null)
        {
            services.AddOptions();
            services.AddHttpContextAccessor();
            services.TryAddSingleton<IRemoteRequestHeaderProvider, HttpContextHeaderProvider>();

            services.AddOptions<NacosOption>()
                .BindConfiguration("Nacos")
                .PostConfigure(option =>
                {
                    configure?.Invoke(option);
                });

            services.AddHttpClient(NacosServiceDiscovery.HttpClientName);
            services.TryAddSingleton<NacosServiceDiscovery>();

            foreach (var iface in RemoteApiRegistrationHelper.FindRemoteInterfaces())
            {
                var attr = iface.GetCustomAttribute<NacosServiceAttribute>();
                if (attr == null)
                    continue;

                if (RemoteApiRegistrationHelper.HasLocalImplementation(services, iface))
                    continue;

                var clientName = iface.FullName!;
                var serviceName = attr.ServiceName;
                var group = attr.Group;

                services.AddHttpClient(clientName, client =>
                    {
                        client.BaseAddress = new Uri("http://nacos.placeholder/");
                    })
                    .AddHttpMessageHandler(sp =>
                        new NacosResolvingHandler(
                            sp.GetRequiredService<NacosServiceDiscovery>(),
                            serviceName,
                            group));

                RemoteApiRegistrationHelper.ReplaceWithInterfaceProxy(services, iface, sp =>
                {
                    var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient(clientName);
                    var headerProvider = sp.GetRequiredService<IRemoteRequestHeaderProvider>();
                    return RemoteApiNacosClientFactory.Create(httpClient, headerProvider, iface);
                });
            }

            return services;
        }
    }
}
