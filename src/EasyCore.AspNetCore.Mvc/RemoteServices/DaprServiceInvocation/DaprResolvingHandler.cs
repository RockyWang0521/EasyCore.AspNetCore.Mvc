namespace EasyCore.AspNetCore.Mvc.RemoteServices
{
    /// <summary>
    /// HTTP message handler that rewrites outbound requests to Dapr service invocation URLs.
    /// </summary>
    internal sealed class DaprResolvingHandler : DelegatingHandler
    {
        private readonly string _appId;

        /// <summary>
        /// Initializes a new instance of the <see cref="DaprResolvingHandler"/> class.
        /// </summary>
        /// <param name="appId">The Dapr app id.</param>
        public DaprResolvingHandler(string appId)
        {
            _appId = appId ?? throw new ArgumentNullException(nameof(appId));
        }

        /// <inheritdoc />
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            request.RequestUri = RewriteRequestUri(request.RequestUri, _appId);
            return base.SendAsync(request, cancellationToken);
        }

        /// <summary>
        /// Rewrites a request URI to the Dapr invoke form.
        /// When the original URI is absolute (HttpClient already applied BaseAddress),
        /// the result stays absolute on the same authority.
        /// </summary>
        internal static Uri RewriteRequestUri(Uri? requestUri, string appId)
        {
            var relative = BuildInvokeUri(requestUri, appId);
            if (requestUri is { IsAbsoluteUri: true })
            {
                var authority = requestUri.GetLeftPart(UriPartial.Authority);
                if (!authority.EndsWith('/'))
                    authority += "/";
                return new Uri(new Uri(authority), relative);
            }

            return relative;
        }

        /// <summary>
        /// Builds a Dapr invoke relative URI: <c>v1.0/invoke/{appId}/method/{pathAndQuery}</c>.
        /// Combined with an <see cref="HttpClient.BaseAddress"/> pointing at the sidecar root.
        /// </summary>
        internal static Uri BuildInvokeUri(Uri? requestUri, string appId)
        {
            if (string.IsNullOrWhiteSpace(appId))
                throw new ArgumentException("Dapr app id is required.", nameof(appId));

            var pathAndQuery = "/";
            if (requestUri != null)
            {
                pathAndQuery = requestUri.IsAbsoluteUri
                    ? requestUri.PathAndQuery
                    : requestUri.OriginalString;

                if (string.IsNullOrWhiteSpace(pathAndQuery))
                    pathAndQuery = "/";
            }

            if (!pathAndQuery.StartsWith('/'))
                pathAndQuery = "/" + pathAndQuery;

            var method = pathAndQuery.TrimStart('/');
            var relative = $"v1.0/invoke/{Uri.EscapeDataString(appId)}/method/{method}";
            return new Uri(relative, UriKind.Relative);
        }
    }
}
