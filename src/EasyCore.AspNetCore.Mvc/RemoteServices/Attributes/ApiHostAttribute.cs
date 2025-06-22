namespace EasyCore.AspNetCore.Mvc.RemoteServices
{
    /// <summary>
    /// Attribute to specify the API host for a remote service.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
    public sealed class ApiHostAttribute : Attribute
    {
        public string ConfigKey { get; set; }

        public ApiHostAttribute(string configKey)
        {
            ConfigKey = configKey ?? throw new ArgumentNullException(nameof(configKey));
        }
    }
}
