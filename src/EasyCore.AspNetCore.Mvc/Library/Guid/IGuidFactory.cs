namespace EasyCore.AspNetCore.Mvc.AppService
{
    public interface IGuidFactory
    {
        /// <summary>
        /// Generates a new GUID.
        /// </summary>
        Guid NewGuid { get; }
    }
}
