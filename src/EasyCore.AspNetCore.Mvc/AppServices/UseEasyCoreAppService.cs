using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EasyCore.AspNetCore.Mvc.AppService
{
    public static class UseEasyCoreAppServices
    {
        public static void EasyCoreAppServices(this IServiceCollection service)
        {
            service.AddHttpContextAccessor();

            service.TryAddTransient<ICurrentUser, CurrentUser>();

            service.TryAddTransient<ICurrentTenant, CurrentTenant>();

            service.TryAddTransient<IGuidFactory, GuidFactory>();

            service.TryAddTransient<ICurrentToken, CurrentToken>();

            service.TryAddTransient<IEasyCoreAppService, EasyCoreAppService>();
        }
    }
}
