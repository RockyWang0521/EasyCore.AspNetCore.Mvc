namespace EasyCore.AspNetCore.Mvc.RemoteServices
{
    /// <summary>
    /// Marks a remote contract to resolve its host address through Consul service discovery.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
    public sealed class ConsulServiceAttribute : Attribute
    {
        /// <summary>
        /// Gets the Consul service name used for discovery.
        /// </summary>
        public string ServiceName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsulServiceAttribute"/> class.
        /// </summary>
        /// <param name="serviceName">The registered Consul service name.</param>
        public ConsulServiceAttribute(string serviceName)
        {
            ServiceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
        }
    }
}
