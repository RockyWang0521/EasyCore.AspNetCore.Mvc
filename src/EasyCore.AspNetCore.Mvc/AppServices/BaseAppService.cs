using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace EasyCore.AspNetCore.Mvc.AppService
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseAppService : ControllerBase
    {
        private IServiceProvider ServiceProvider => HttpContext.RequestServices;

        protected ICurrentUser CurrentUser => ServiceProvider.GetRequiredService<ICurrentUser>();

        protected ICurrentTenant CurrentTenant => ServiceProvider.GetRequiredService<ICurrentTenant>();

        protected IMapper Mapper => ServiceProvider.GetRequiredService<IMapper>();

        protected IGuidFactory GuidFactory => ServiceProvider.GetRequiredService<IGuidFactory>();

        protected ICurrentToken CurrentToken => ServiceProvider.GetRequiredService<ICurrentToken>();

        public BaseAppService()
        {

        }
    }
}
