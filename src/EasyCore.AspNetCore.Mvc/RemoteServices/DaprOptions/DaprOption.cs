namespace EasyCore.AspNetCore.Mvc.RemoteServices.DaprOptions
{
    /// <summary>
    /// Options for invoking remote APIs through the Dapr HTTP sidecar.
    /// </summary>
    public class DaprOption
    {
        /// <summary>
        /// Gets or sets the Dapr HTTP sidecar base address
        /// (for example, <c>http://127.0.0.1:3500/</c>).
        /// When empty, resolution order is:
        /// <c>DAPR_HTTP_ENDPOINT</c> → <c>DAPR_HTTP_PORT</c> → <c>http://127.0.0.1:3500/</c>.
        /// </summary>
        public string HttpEndpoint { get; set; } = string.Empty;

        /// <summary>
        /// Resolves the sidecar base URI, ensuring a trailing slash.
        /// </summary>
        public Uri ResolveHttpEndpoint()
        {
            var endpoint = HttpEndpoint;
            if (string.IsNullOrWhiteSpace(endpoint))
                endpoint = Environment.GetEnvironmentVariable("DAPR_HTTP_ENDPOINT");

            if (string.IsNullOrWhiteSpace(endpoint))
            {
                var port = Environment.GetEnvironmentVariable("DAPR_HTTP_PORT");
                endpoint = string.IsNullOrWhiteSpace(port)
                    ? "http://127.0.0.1:3500/"
                    : $"http://127.0.0.1:{port}/";
            }

            if (!endpoint.EndsWith('/'))
                endpoint += "/";

            return new Uri(endpoint);
        }
    }
}
