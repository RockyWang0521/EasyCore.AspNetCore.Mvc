using EasyCore.AspNetCore.Mvc.RemoteServices.K8sOptions;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace EasyCore.AspNetCore.Mvc.RemoteServices
{
    public static class RemoteApiK8sClientExtensions
    {
        public static IServiceCollection EasyCoreK8sRemoteApiClients(this IServiceCollection services, Action<K8sOption> action)
        {
            services.AddOptions();

            services.Configure(action);

            var serviceProvider = services.BuildServiceProvider();

            var option = serviceProvider.GetService<IOptions<K8sOption>>()?.Value;

            if (option == null)
            {
                throw new InvalidOperationException("K8sOption is not configured.");
            }

            var allTypes = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)
                .SelectMany(a => a.GetTypes());

            var interfaces = allTypes
                .Where(t => t.IsInterface && typeof(IRemoteAppService).IsAssignableFrom(t))
                .Where(t => t.GetCustomAttribute<K8sDnsAttribute>() != null);

            foreach (var iface in interfaces)
            {
                var attr = iface.GetCustomAttribute<K8sDnsAttribute>()!;

                var fullDns = $"http://{attr.ServiceName}.{option.K8sNamespace}.{option.K8sClusterDomain}";

                services.AddHttpClient(iface.FullName!, c => c.BaseAddress = new Uri(fullDns));

                services.AddTransient(iface, sp =>
                {
                    var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient(iface.FullName!);

                    var headerProvider = sp.GetService<IRemoteRequestHeaderProvider>();

                    var method = typeof(RemoteApiK8sClientFactory).GetMethod(nameof(RemoteApiK8sClientFactory.Create))!
                        .MakeGenericMethod(iface);

                    return method.Invoke(null, new object[] { client, headerProvider! })!;
                });
            }

            return services;
        }
    }
}
