namespace EasyCore.AspNetCore.Mvc.AppService
{
    /// <summary>
    /// Provides access to the current tenant identifier.
    /// </summary>
    public interface ICurrentTenant
    {
        /// <summary>
        /// Gets the current tenant identifier from HTTP context items or the <c>X-Tenant-Id</c> header.
        /// </summary>
        string? TenantId { get; }
    }
}
