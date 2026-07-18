namespace EasyCore.AspNetCore.Mvc.RemoteServices
{
    /// <summary>
    /// Marks a remote contract to resolve its host through Kubernetes DNS
    /// (<c>http://{ServiceName}.{Namespace}.svc.{ClusterDomain}[:Port]</c>).
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
    public sealed class K8sDnsAttribute : Attribute
    {
        /// <summary>
        /// Gets the Kubernetes service name used in the DNS host.
        /// </summary>
        public string ServiceName { get; }

        /// <summary>
        /// Gets the optional TCP port. When null or 80, the port is omitted from the base URL.
        /// </summary>
        public int? Port { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="K8sDnsAttribute"/> class.
        /// </summary>
        /// <param name="serviceName">The Kubernetes service name.</param>
        /// <param name="port">Optional service port. Omit or use 80 for the default HTTP port.</param>
        public K8sDnsAttribute(string serviceName, int port = 80)
        {
            ServiceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
            Port = port <= 0 ? null : port;
        }
    }
}
