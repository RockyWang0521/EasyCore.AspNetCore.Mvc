namespace EasyCore.AspNetCore.Mvc.RemoteServices
{
    [AttributeUsage(AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
    public sealed class ConsulServiceAttribute : Attribute
    {
        public string ServiceName { get; }

        public ConsulServiceAttribute(string serviceName)
        {
            ServiceName = serviceName;
        }
    }

}
