namespace EasyCore.AspNetCore.Mvc.RemoteServices
{
    /// <summary>
    /// Forwards selected headers from the current HTTP context to remote API calls.
    /// </summary>
    public class HttpContextHeaderProvider : IRemoteRequestHeaderProvider
    {
        /// <summary>
        /// Provides access to the current HTTP context.
        /// </summary>
        private readonly IHttpContextAccessor _accessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpContextHeaderProvider"/> class.
        /// </summary>
        /// <param name="accessor">The HTTP context accessor.</param>
        public HttpContextHeaderProvider(IHttpContextAccessor accessor) => _accessor = accessor;

        /// <summary>
        /// Collects <c>Authorization</c>, <c>X-Tenant-Id</c>, and <c>X-Trace-Id</c> headers when present.
        /// </summary>
        /// <returns>A dictionary of headers to forward; empty when there is no HTTP context.</returns>
        public IDictionary<string, string> GetHeaders()
        {
            var ctx = _accessor.HttpContext;

            if (ctx == null) return new Dictionary<string, string>();

            var headers = new Dictionary<string, string>();

            if (ctx.Request.Headers.TryGetValue("Authorization", out var auth))
                headers["Authorization"] = auth!;

            if (ctx.Request.Headers.TryGetValue("X-Tenant-Id", out var tenant))
                headers["X-Tenant-Id"] = tenant!;

            if (ctx.Request.Headers.TryGetValue("X-Trace-Id", out var trace))
                headers["X-Trace-Id"] = trace!;

            return headers;
        }
    }
}
