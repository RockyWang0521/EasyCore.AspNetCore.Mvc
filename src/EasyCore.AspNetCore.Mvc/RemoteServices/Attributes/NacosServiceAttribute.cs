namespace EasyCore.AspNetCore.Mvc.RemoteServices
{
    /// <summary>
    /// Marks a remote contract to resolve its host address through Nacos service discovery.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
    public sealed class NacosServiceAttribute : Attribute
    {
        /// <summary>
        /// Gets the Nacos service name used for discovery.
        /// </summary>
        public string ServiceName { get; }

        /// <summary>
        /// Gets the Nacos group name. When null, the configured default group is used.
        /// </summary>
        public string? Group { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NacosServiceAttribute"/> class.
        /// </summary>
        /// <param name="serviceName">The registered Nacos service name.</param>
        /// <param name="group">Optional group name; defaults to configuration or <c>DEFAULT_GROUP</c>.</param>
        public NacosServiceAttribute(string serviceName, string? group = null)
        {
            ServiceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
            Group = group;
        }
    }
}
