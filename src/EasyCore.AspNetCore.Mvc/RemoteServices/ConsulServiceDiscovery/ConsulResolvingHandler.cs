namespace EasyCore.AspNetCore.Mvc.RemoteServices
{
    /// <summary>
    /// HTTP message handler that rewrites outbound requests to a healthy Consul service instance.
    /// </summary>
    internal sealed class ConsulResolvingHandler : DelegatingHandler
    {
        /// <summary>
        /// The Consul discovery service used to resolve instance addresses.
        /// </summary>
        private readonly ConsulServiceDiscovery _discovery;

        /// <summary>
        /// The Consul service name to resolve.
        /// </summary>
        private readonly string _serviceName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsulResolvingHandler"/> class.
        /// </summary>
        /// <param name="discovery">The Consul service discovery component.</param>
        /// <param name="serviceName">The Consul service name.</param>
        public ConsulResolvingHandler(ConsulServiceDiscovery discovery, string serviceName)
        {
            _discovery = discovery;
            _serviceName = serviceName;
        }

        /// <summary>
        /// Resolves the Consul service base URI and forwards the request.
        /// </summary>
        /// <param name="request">The outbound HTTP request.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The HTTP response from the remote service.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var baseUri = await _discovery.GetServiceUriAsync(_serviceName).ConfigureAwait(false)
                ?? throw new InvalidOperationException($"Service {_serviceName} not found in Consul");

            var relative = request.RequestUri;
            if (relative != null && relative.IsAbsoluteUri)
            {
                relative = new Uri(relative.PathAndQuery, UriKind.Relative);
            }

            request.RequestUri = relative == null
                ? baseUri
                : new Uri(baseUri, relative);

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
