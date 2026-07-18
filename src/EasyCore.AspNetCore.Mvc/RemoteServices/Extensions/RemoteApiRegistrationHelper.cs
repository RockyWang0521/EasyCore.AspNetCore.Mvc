using System.Reflection;

namespace EasyCore.AspNetCore.Mvc.RemoteServices
{
    /// <summary>
    /// Shared helpers for scanning remote interfaces and registering interface-only proxies.
    /// </summary>
    internal static class RemoteApiRegistrationHelper
    {
        /// <summary>
        /// Finds all loaded remote app-service interfaces except <see cref="IRemoteAppService"/> itself.
        /// </summary>
        /// <returns>An enumerable of interface types that implement <see cref="IRemoteAppService"/>.</returns>
        public static IEnumerable<Type> FindRemoteInterfaces()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)
                .SelectMany(SafeGetTypes)
                .Where(t => t.IsInterface
                            && typeof(IRemoteAppService).IsAssignableFrom(t)
                            && t != typeof(IRemoteAppService));
        }

        /// <summary>
        /// Tries to read the already-registered <see cref="IConfiguration"/> instance from the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The configuration instance, or <c>null</c> when it is not available as an instance registration.</returns>
        public static IConfiguration? TryGetConfiguration(IServiceCollection services)
        {
            foreach (var descriptor in services)
            {
                if (descriptor.ServiceType == typeof(IConfiguration)
                    && descriptor.ImplementationInstance is IConfiguration configuration)
                {
                    return configuration;
                }
            }

            return null;
        }

        /// <summary>
        /// Determines whether a concrete local implementation of the remote interface exists
        /// in DI descriptors or in currently loaded assemblies.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="iface">The remote contract interface.</param>
        /// <returns><c>true</c> when a concrete implementation type is found; otherwise <c>false</c>.</returns>
        public static bool HasLocalImplementation(IServiceCollection services, Type iface)
        {
            foreach (var descriptor in services)
            {
                if (descriptor.ImplementationType is Type impl
                    && !impl.IsInterface
                    && !impl.IsAbstract
                    && iface.IsAssignableFrom(impl))
                {
                    return true;
                }

                // Some DI registrations map the concrete type to itself.
                if (descriptor.ServiceType is Type serviceType
                    && serviceType.IsClass
                    && !serviceType.IsAbstract
                    && iface.IsAssignableFrom(serviceType))
                {
                    return true;
                }
            }

            // Fallback: host apps may expose AppServices via MVC without an interface DI mapping yet.
            return FindRemoteInterfaces()
                .SelectMany(_ => AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic).SelectMany(SafeGetTypes))
                .Any(t => t.IsClass && !t.IsAbstract && iface.IsAssignableFrom(t));
        }

        /// <summary>
        /// Checks whether <c>RemoteServices:{configKey}</c> has a non-empty value.
        /// </summary>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="configKey">The remote service configuration key.</param>
        /// <returns><c>true</c> when a remote base URL is configured; otherwise <c>false</c>.</returns>
        public static bool HasApiHostConfig(IConfiguration? configuration, string configKey)
        {
            if (configuration == null || string.IsNullOrWhiteSpace(configKey))
                return false;

            return !string.IsNullOrWhiteSpace(configuration[$"RemoteServices:{configKey}"]);
        }

        /// <summary>
        /// Decides whether a remote proxy should be registered for the given interface.
        /// For ApiHost, skips when a local implementation exists and remote URL is not configured.
        /// For Consul/K8s, skips whenever a local implementation exists.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="iface">The remote contract interface.</param>
        /// <param name="requireRemoteConfig">
        /// When <c>true</c>, remote configuration presence is considered (ApiHost mode).
        /// </param>
        /// <param name="hasRemoteConfig">Whether remote endpoint configuration is available.</param>
        /// <returns><c>true</c> when an interface proxy should be registered; otherwise <c>false</c>.</returns>
        public static bool ShouldRegisterRemoteProxy(
            IServiceCollection services,
            Type iface,
            bool requireRemoteConfig,
            bool hasRemoteConfig)
        {
            var hasLocal = HasLocalImplementation(services, iface);

            if (hasLocal && requireRemoteConfig && !hasRemoteConfig)
                return false;

            if (hasLocal && !requireRemoteConfig)
                return false;

            return true;
        }

        /// <summary>
        /// Removes existing registrations for <paramref name="iface"/> and registers an interface-only proxy factory.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="iface">The remote contract interface.</param>
        /// <param name="factory">Factory that creates the DispatchProxy instance.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="iface"/> is not a valid remote interface.</exception>
        public static void ReplaceWithInterfaceProxy(
            IServiceCollection services,
            Type iface,
            Func<IServiceProvider, object> factory)
        {
            if (!iface.IsInterface)
                throw new ArgumentException($"Remote API proxy requires an interface: {iface.FullName}", nameof(iface));

            if (!typeof(IRemoteAppService).IsAssignableFrom(iface) || iface == typeof(IRemoteAppService))
                throw new ArgumentException($"Type must be a remote app-service interface: {iface.FullName}", nameof(iface));

            var existing = services.Where(d => d.ServiceType == iface).ToList();
            foreach (var descriptor in existing)
                services.Remove(descriptor);

            services.AddTransient(iface, factory);
        }

        /// <summary>
        /// Safely enumerates types from an assembly, tolerating reflection load failures.
        /// </summary>
        /// <param name="assembly">The assembly to inspect.</param>
        /// <returns>Loaded types from the assembly.</returns>
        private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(t => t != null)!;
            }
        }
    }
}
