namespace EasyCore.AspNetCore.Mvc.AppService
{
    /// <summary>
    /// Default application service base type that users should inherit for dynamic API generation.
    /// </summary>
    public class EasyCoreAppService : BaseAppService, IEasyCoreAppService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EasyCoreAppService"/> class.
        /// </summary>
        public EasyCoreAppService()
        {
        }
    }
}
