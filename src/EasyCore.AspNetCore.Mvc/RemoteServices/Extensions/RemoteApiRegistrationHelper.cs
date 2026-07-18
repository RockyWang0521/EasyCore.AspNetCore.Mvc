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
        public static IEnumerable<Type> FindRemoteInterfaces()
        {
            EnsureReferencedAssembliesLoaded();

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
        /// Determines whether a concrete local implementation of the remote interface exists.
        /// </summary>
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

                if (descriptor.ServiceType is Type serviceType
                    && serviceType.IsClass
                    && !serviceType.IsAbstract
                    && iface.IsAssignableFrom(serviceType))
                {
                    return true;
                }
            }

            EnsureReferencedAssembliesLoaded();

            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)
                .SelectMany(SafeGetTypes)
                .Any(t => t.IsClass && !t.IsAbstract && iface.IsAssignableFrom(t));
        }

        /// <summary>
        /// Checks whether <c>RemoteServices:{configKey}</c> has a non-empty value.
        /// </summary>
        public static bool HasApiHostConfig(IConfiguration? configuration, string configKey)
        {
            if (configuration == null || string.IsNullOrWhiteSpace(configKey))
                return false;

            return !string.IsNullOrWhiteSpace(configuration[$"RemoteServices:{configKey}"]);
        }

        /// <summary>
        /// Decides whether a remote proxy should be registered for the given interface.
        /// </summary>
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
        /// Eagerly loads referenced assemblies so contract interfaces are discoverable at registration time.
        /// </summary>
        private static void EnsureReferencedAssembliesLoaded()
        {
            var loadedNames = new HashSet<string>(
                AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.IsDynamic)
                    .Select(a => a.GetName().Name!)
                    .Where(n => !string.IsNullOrEmpty(n)),
                StringComparer.OrdinalIgnoreCase);

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic).ToArray())
            {
                AssemblyName[] references;
                try
                {
                    references = assembly.GetReferencedAssemblies();
                }
                catch
                {
                    continue;
                }

                foreach (var reference in references)
                {
                    if (reference.Name == null || loadedNames.Contains(reference.Name))
                        continue;

                    // Skip framework assemblies — they never contain user remote contracts.
                    if (reference.Name.StartsWith("System.", StringComparison.OrdinalIgnoreCase)
                        || reference.Name.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase)
                        || reference.Name.Equals("netstandard", StringComparison.OrdinalIgnoreCase)
                        || reference.Name.Equals("mscorlib", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    try
                    {
                        Assembly.Load(reference);
                        loadedNames.Add(reference.Name);
                    }
                    catch
                    {
                        // Ignore assemblies that cannot be loaded in this context.
                    }
                }
            }
        }

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
