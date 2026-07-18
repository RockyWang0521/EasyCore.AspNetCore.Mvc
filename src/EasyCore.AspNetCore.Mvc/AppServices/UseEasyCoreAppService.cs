using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EasyCore.AspNetCore.Mvc.AppService
{
    /// <summary>
    /// Extension methods for registering EasyCore application-service helpers.
    /// </summary>
    public static class UseEasyCoreAppServices
    {
        /// <summary>
        /// Registers Mapster, current-user/tenant/token accessors, and GUID factory services.
        /// </summary>
        /// <param name="service">The service collection to configure.</param>
        public static void EasyCoreAppServices(this IServiceCollection service)
        {
            service.AddHttpContextAccessor();

            var config = TypeAdapterConfig.GlobalSettings;
            service.TryAddSingleton(config);
            service.TryAddScoped<IMapper, ServiceMapper>();

            service.TryAddTransient<ICurrentUser, CurrentUser>();
            service.TryAddTransient<ICurrentTenant, CurrentTenant>();
            service.TryAddTransient<IGuidFactory, GuidFactory>();
            service.TryAddTransient<ICurrentToken, CurrentToken>();
            service.TryAddTransient<IEasyCoreAppService, EasyCoreAppService>();
        }
    }
}
