namespace EasyCore.AspNetCore.Mvc.AppService
{
    internal class CurrentTenant : ICurrentTenant
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public string TenantId => _httpContextAccessor.HttpContext!.Items["TenantId"]?.ToString() ?? string.Empty;

        public CurrentTenant(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;
    }
}
