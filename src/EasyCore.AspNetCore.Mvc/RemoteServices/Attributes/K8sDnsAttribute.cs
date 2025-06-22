namespace EasyCore.AspNetCore.Mvc.RemoteServices
{
    /// <summary>
    /// Attribute to mark a service as a DNS service for Kubernetes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
    public sealed class K8sDnsAttribute : Attribute
    {
        public string ServiceName { get; }

        public K8sDnsAttribute(string serviceName)
        {
            ServiceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
        }
    }
}
