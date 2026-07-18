namespace EasyCore.AspNetCore.Mvc.AppService
{
    /// <summary>
    /// Default <see cref="ICurrentTenant"/> implementation that reads tenant id from items or request headers.
    /// </summary>
    internal class CurrentTenant : ICurrentTenant
    {
        /// <summary>
        /// Provides access to the current HTTP context.
        /// </summary>
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentTenant"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        public CurrentTenant(IHttpContextAccessor httpContextAccessor)
            => _httpContextAccessor = httpContextAccessor;

        /// <inheritdoc />
        public string? TenantId
        {
            get
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null)
                    return null;

                if (httpContext.Items.TryGetValue("TenantId", out var item) && item != null)
                    return item.ToString();

                if (httpContext.Request.Headers.TryGetValue("X-Tenant-Id", out var header))
                {
                    var value = header.ToString();
                    return string.IsNullOrWhiteSpace(value) ? null : value;
                }

                return null;
            }
        }
    }
}
