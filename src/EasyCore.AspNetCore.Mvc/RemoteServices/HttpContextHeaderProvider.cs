namespace EasyCore.AspNetCore.Mvc.RemoteServices
{
    /// <summary>
    /// Provides the headers from the current HTTP context.
    /// </summary>
    public class HttpContextHeaderProvider : IRemoteRequestHeaderProvider
    {
        private readonly IHttpContextAccessor _accessor;

        public HttpContextHeaderProvider(IHttpContextAccessor accessor) => _accessor = accessor;

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