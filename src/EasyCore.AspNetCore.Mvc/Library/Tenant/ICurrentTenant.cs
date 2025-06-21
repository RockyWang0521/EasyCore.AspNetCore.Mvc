namespace EasyCore.AspNetCore.Mvc.AppService
{
    public interface ICurrentTenant
    {
        /// <summary>
        /// Get the current tenant identifier.
        /// </summary>
        string? TenantId { get; }
    }
}
