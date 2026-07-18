namespace EasyCore.AspNetCore.Mvc.AppService
{
    /// <summary>
    /// Factory for generating sequential GUIDs suitable for database primary keys.
    /// </summary>
    public interface IGuidFactory
    {
        /// <summary>
        /// Gets a newly generated sequential GUID.
        /// </summary>
        Guid NewGuid { get; }
    }
}
