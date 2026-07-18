namespace EasyCore.AspNetCore.Mvc.RemoteServices.NacosOptions
{
    /// <summary>
    /// Options used when discovering remote API hosts through Nacos Naming OpenAPI.
    /// </summary>
    public class NacosOption
    {
        /// <summary>
        /// Gets or sets one or more Nacos server addresses, comma-separated
        /// (for example, <c>http://localhost:8848</c>).
        /// </summary>
        public string ServerAddresses { get; set; } = "http://localhost:8848";

        /// <summary>
        /// Gets or sets the Nacos namespace id. Empty means the public namespace.
        /// </summary>
        public string Namespace { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the default group name when the attribute does not specify one.
        /// </summary>
        public string GroupName { get; set; } = "DEFAULT_GROUP";

        /// <summary>
        /// Gets or sets the optional Nacos username for authenticated OpenAPI access.
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// Gets or sets the optional Nacos password for authenticated OpenAPI access.
        /// </summary>
        public string? Password { get; set; }
    }
}
