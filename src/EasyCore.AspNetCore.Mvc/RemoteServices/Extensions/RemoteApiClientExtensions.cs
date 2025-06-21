using System.Reflection;

namespace EasyCore.AspNetCore.Mvc.RemoteServices
{
    /// <summary>
    /// Extension methods for <see cref="IServiceCollection"/> to register remote API clients.
    /// </summary>

    public static class RemoteApiClientExtensions
    {
        public static IServiceCollection EasyCoreRemoteApiClients(this IServiceCollection services)
        {
            services.AddSingleton<IRemoteRequestHeaderProvider, HttpContextHeaderProvider>();

            RegisterClients(services, (iface, attr, svc) =>
            {
                svc.AddHttpClient(iface.FullName!, (sp, c) =>
                {
                    var config = sp.GetRequiredService<IConfiguration>();

                    var baseUrl = !string.IsNullOrWhiteSpace(attr.ConfigKey)
                        ? config[$"RemoteServices:{attr.ConfigKey}"]
                        : throw new InvalidOperationException($"Interface {iface.Name} is missing ConfigKey.");

                    if (string.IsNullOrWhiteSpace(baseUrl))
                        throw new InvalidOperationException($"Configure RemoteServices: {attr.ConfigKey} is null or empty.");

                    c.BaseAddress = new Uri(baseUrl);
                });

                svc.AddTransient(iface, sp =>
                {
                    var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient(iface.FullName!);

                    var headerProvider = sp.GetRequiredService<IRemoteRequestHeaderProvider>();

                    return RemoteApiHostClientFactory.Create(httpClient, null, headerProvider, iface);
                });
            });

            return services;
        }

        private static void RegisterClients(IServiceCollection services, Action<Type, ApiHostAttribute, IServiceCollection> registerHttpClient)
        {
            var allTypes = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)
                .SelectMany(a => a.GetTypes());

            var interfaces = allTypes
                .Where(t => t.IsInterface && typeof(IRemoteAppService).IsAssignableFrom(t) && t != typeof(IRemoteAppService));

            foreach (var iface in interfaces)
            {
                var attr = iface.GetCustomAttribute<ApiHostAttribute>();

                if (attr == null) continue;

                registerHttpClient(iface, attr, services);
            }
        }
    }
}
