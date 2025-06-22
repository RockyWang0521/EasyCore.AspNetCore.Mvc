namespace EasyCore.AspNetCore.Mvc.RemoteServices
{
    /// <summary>
    /// Attribute to mark an interface as a remote service that should be registered with Consul.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
    public sealed class ConsulServiceAttribute : Attribute
    {
        public string ServiceName { get; }

        public ConsulServiceAttribute(string serviceName)
        {
            ServiceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
        }
    }
}
