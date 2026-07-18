using MapsterMapper;
using Microsoft.AspNetCore.Mvc;

namespace EasyCore.AspNetCore.Mvc.AppService
{
    /// <summary>
    /// Base class for application services exposed as API controllers.
    /// Provides request-scoped helpers for the current user, tenant, token, GUID factory, and mapper.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseAppService : ControllerBase
    {
        /// <summary>
        /// Gets the request-scoped <see cref="IServiceProvider"/> from the current HTTP context.
        /// </summary>
        private IServiceProvider ServiceProvider => HttpContext.RequestServices;

        /// <summary>
        /// Gets the current authenticated user information.
        /// </summary>
        protected ICurrentUser CurrentUser => ServiceProvider.GetRequiredService<ICurrentUser>();

        /// <summary>
        /// Gets the current tenant information.
        /// </summary>
        protected ICurrentTenant CurrentTenant => ServiceProvider.GetRequiredService<ICurrentTenant>();

        /// <summary>
        /// Gets the Mapster mapper used for object mapping.
        /// </summary>
        protected IMapper Mapper => ServiceProvider.GetRequiredService<IMapper>();

        /// <summary>
        /// Gets the sequential GUID factory.
        /// </summary>
        protected IGuidFactory GuidFactory => ServiceProvider.GetRequiredService<IGuidFactory>();

        /// <summary>
        /// Gets the bearer token from the current request, if present.
        /// </summary>
        protected ICurrentToken CurrentToken => ServiceProvider.GetRequiredService<ICurrentToken>();
    }
}
