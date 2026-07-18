namespace EasyCore.AspNetCore.Mvc.RemoteServices
{
    /// <summary>
    /// Marks a remote contract to use a static host URL from configuration
    /// under <c>RemoteServices:{ConfigKey}</c>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
    public sealed class ApiHostAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the configuration key used to resolve the remote base URL.
        /// </summary>
        public string ConfigKey { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiHostAttribute"/> class.
        /// </summary>
        /// <param name="configKey">The configuration key under <c>RemoteServices</c>.</param>
        public ApiHostAttribute(string configKey)
        {
            ConfigKey = configKey ?? throw new ArgumentNullException(nameof(configKey));
        }
    }
}
