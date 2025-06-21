using Consul;
using System.Reflection;

namespace EasyCore.AspNetCore.Mvc.RemoteServices
{
    /// <summary>
    /// Extension methods for registering remote API clients with Consul.
    /// </summary>
    public static class RemoteApiConsulClientExtensions
    {
        public static IServiceCollection EasyCoreRemoteApiConsulClients(this IServiceCollection services)
        {
            using var provider = services.BuildServiceProvider();

            var Configuration = provider.GetRequiredService<IConfiguration>();

            var consulAddress = Configuration["Consul:ConsulAddress"];

            if (string.IsNullOrEmpty(consulAddress)) throw new ArgumentException("Consul address is not configured.");

            services.AddSingleton<IConsulClient>(sp => new ConsulClient(config =>
            {
                config.Address = new Uri(consulAddress);
            }));

            services.AddSingleton<ConsulServiceDiscovery>();

            services.AddSingleton<IRemoteRequestHeaderProvider, HttpContextHeaderProvider>();

            RegisterClients(services, async (iface, attr, sp) =>
            {
                var discovery = sp.GetRequiredService<ConsulServiceDiscovery>();

                var headerProvider = sp.GetRequiredService<IRemoteRequestHeaderProvider>();

                var proxy = await RemoteApiConsulClientFactory.CreateAsync(iface, discovery, headerProvider);

                return proxy;
            });

            return services;
        }

        /// <summary>
        /// Register remote API clients with Consul.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="createProxyAsync"></param>
        private static void RegisterClients(IServiceCollection services, Func<Type, ConsulServiceAttribute, IServiceProvider, Task<object>> createProxyAsync)
        {
            var allTypes = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)
                .SelectMany(a => a.GetTypes());

            var interfaces = allTypes
                .Where(t => t.IsInterface && typeof(IRemoteAppService).IsAssignableFrom(t) && t != typeof(IRemoteAppService));

            foreach (var iface in interfaces)
            {
                var attr = iface.GetCustomAttribute<ConsulServiceAttribute>();
                if (attr == null) continue;

                services.AddTransient(iface, sp =>
                {
                    return createProxyAsync(iface, attr, sp).GetAwaiter().GetResult();
                });
            }
        }
    }

}
