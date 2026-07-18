namespace EasyCore.AspNetCore.Mvc.RemoteServices
{
    /// <summary>
    /// Marks a remote contract to resolve its host through Kubernetes DNS
    /// (<c>http://{ServiceName}.{Namespace}.{ClusterDomain}</c>).
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
    public sealed class K8sDnsAttribute : Attribute
    {
        /// <summary>
        /// Gets the Kubernetes service name used in the DNS host.
        /// </summary>
        public string ServiceName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="K8sDnsAttribute"/> class.
        /// </summary>
        /// <param name="serviceName">The Kubernetes service name.</param>
        public K8sDnsAttribute(string serviceName)
        {
            ServiceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
        }
    }
}
