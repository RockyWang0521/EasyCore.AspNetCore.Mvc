using System.Reflection;
using EasyCore.AspNetCore.Mvc.RemoteServices.K8sOptions;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

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
        /// Reads <c>K8s:*</c> from configuration when <paramref name="configure"/> is omitted.
        /// </summary>
        public static IServiceCollection AddEasyCoreRemoteApiK8sClients(
            this IServiceCollection services,
            Action<K8sOption>? configure = null)
        {
            services.AddOptions();
            services.AddHttpContextAccessor();
            services.TryAddSingleton<IRemoteRequestHeaderProvider, HttpContextHeaderProvider>();
            RemoteInterceptorComposer.RegisterCore(services);

            services.AddOptions<K8sOption>()
                .BindConfiguration("K8s")
                .PostConfigure(o => configure?.Invoke(o));

            foreach (var iface in RemoteApiRegistrationHelper.FindRemoteInterfaces())
            {
                var attr = iface.GetCustomAttribute<K8sDnsAttribute>();
                if (attr == null)
                    continue;

                if (RemoteApiRegistrationHelper.HasLocalImplementation(services, iface))
                    continue;

                var clientName = iface.FullName!;
                var serviceName = attr.ServiceName;
                var port = attr.Port;

                services.AddHttpClient(clientName, (sp, client) =>
                    {
                        var option = sp.GetRequiredService<IOptionsMonitor<K8sOption>>().CurrentValue;
                        if (string.IsNullOrWhiteSpace(option.K8sNamespace)
                            || string.IsNullOrWhiteSpace(option.K8sClusterDomain))
                        {
                            throw new InvalidOperationException(
                                "K8sOption is not configured. Set K8s:K8sNamespace / K8s:K8sClusterDomain or pass configure.");
                        }

                        var clusterDomain = option.K8sClusterDomain.Trim().Trim('.');
                        if (clusterDomain.StartsWith("svc.", StringComparison.OrdinalIgnoreCase))
                            clusterDomain = clusterDomain["svc.".Length..];

                        client.BaseAddress = BuildK8sBaseAddress(
                            serviceName,
                            option.K8sNamespace,
                            clusterDomain,
                            port);
                    });

                RemoteApiRegistrationHelper.ReplaceWithInterfaceProxy(services, iface, sp =>
                {
                    var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient(clientName);
                    var headerProvider = sp.GetRequiredService<IRemoteRequestHeaderProvider>();
                    var dispatchProxy = RemoteApiK8sClientFactory.Create(httpClient, headerProvider, iface);
                    return RemoteInterceptorComposer.Wrap(iface, dispatchProxy, sp);
                });
            }

            return services;
        }

        /// <summary>
        /// Builds a Kubernetes service DNS base address ending with a trailing slash.
        /// Format: <c>http://{service}.{namespace}.svc.{clusterDomain}[:port]/</c>
        /// </summary>
        internal static Uri BuildK8sBaseAddress(string serviceName, string @namespace, string clusterDomain, int? port)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
                throw new ArgumentException("Kubernetes service name is required.", nameof(serviceName));
            if (string.IsNullOrWhiteSpace(@namespace))
                throw new ArgumentException("Kubernetes namespace is required.", nameof(@namespace));
            if (string.IsNullOrWhiteSpace(clusterDomain))
                throw new ArgumentException("Kubernetes cluster domain is required.", nameof(clusterDomain));

            var domain = clusterDomain.Trim().Trim('.');
            if (domain.StartsWith("svc.", StringComparison.OrdinalIgnoreCase))
                domain = domain["svc.".Length..];

            var host = $"{serviceName.Trim()}.{@namespace.Trim()}.svc.{domain}";
            var authority = port is null or 80 ? host : $"{host}:{port.Value}";
            return new Uri($"http://{authority}/");
        }
    }
}
