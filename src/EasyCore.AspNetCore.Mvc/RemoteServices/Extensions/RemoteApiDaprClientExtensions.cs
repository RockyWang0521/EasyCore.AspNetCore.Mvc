using System.Reflection;
using EasyCore.AspNetCore.Mvc.RemoteServices.DaprOptions;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace EasyCore.AspNetCore.Mvc.RemoteServices
{
    /// <summary>
    /// Extension methods for registering remote API clients that invoke services through Dapr.
    /// </summary>
    public static class RemoteApiDaprClientExtensions
    {
        /// <summary>
        /// Registers interface-only proxies for contracts marked with <see cref="DaprAppAttribute"/>.
        /// Skips interfaces that already have a local implementation.
        /// </summary>
        public static IServiceCollection EasyCoreRemoteApiDaprClients(
            this IServiceCollection services,
            Action<DaprOption>? configure = null)
        {
            services.AddOptions();
            services.AddHttpContextAccessor();
            services.TryAddSingleton<IRemoteRequestHeaderProvider, HttpContextHeaderProvider>();

            services.AddOptions<DaprOption>()
                .BindConfiguration("Dapr")
                .PostConfigure(o => configure?.Invoke(o));

            foreach (var iface in RemoteApiRegistrationHelper.FindRemoteInterfaces())
            {
                var attr = iface.GetCustomAttribute<DaprAppAttribute>();
                if (attr == null)
                    continue;

                if (RemoteApiRegistrationHelper.HasLocalImplementation(services, iface))
                    continue;

                var clientName = iface.FullName!;
                var appId = attr.AppId;

                services.AddHttpClient(clientName, (sp, client) =>
                    {
                        client.BaseAddress = sp.GetRequiredService<IOptionsMonitor<DaprOption>>()
                            .CurrentValue
                            .ResolveHttpEndpoint();
                    })
                    .AddHttpMessageHandler(() => new DaprResolvingHandler(appId));

                RemoteApiRegistrationHelper.ReplaceWithInterfaceProxy(services, iface, sp =>
                {
                    var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient(clientName);
                    var headerProvider = sp.GetRequiredService<IRemoteRequestHeaderProvider>();
                    return RemoteApiDaprClientFactory.Create(httpClient, headerProvider, iface);
                });
            }

            return services;
        }

        /// <summary>
        /// Builds a Dapr invoke relative URI for tests and diagnostics.
        /// </summary>
        internal static Uri BuildDaprInvokeRelativeUri(string appId, string pathAndQuery)
            => DaprResolvingHandler.BuildInvokeUri(
                new Uri(pathAndQuery.StartsWith('/') ? pathAndQuery : "/" + pathAndQuery, UriKind.Relative),
                appId);
    }
}
