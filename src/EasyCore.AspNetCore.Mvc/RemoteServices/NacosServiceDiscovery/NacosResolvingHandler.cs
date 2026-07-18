namespace EasyCore.AspNetCore.Mvc.RemoteServices
{
    /// <summary>
    /// HTTP message handler that rewrites outbound requests to a healthy Nacos service instance.
    /// </summary>
    internal sealed class NacosResolvingHandler : DelegatingHandler
    {
        private readonly NacosServiceDiscovery _discovery;
        private readonly string _serviceName;
        private readonly string? _group;

        /// <summary>
        /// Initializes a new instance of the <see cref="NacosResolvingHandler"/> class.
        /// </summary>
        public NacosResolvingHandler(NacosServiceDiscovery discovery, string serviceName, string? group)
        {
            _discovery = discovery;
            _serviceName = serviceName;
            _group = group;
        }

        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var baseUri = await _discovery.GetServiceUriAsync(_serviceName, _group, cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidOperationException($"Service {_serviceName} not found in Nacos");

            var relative = request.RequestUri;
            if (relative != null && relative.IsAbsoluteUri)
                relative = new Uri(relative.PathAndQuery, UriKind.Relative);

            request.RequestUri = relative == null
                ? baseUri
                : new Uri(baseUri, relative);

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
