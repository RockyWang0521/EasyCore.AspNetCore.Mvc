namespace EasyCore.AspNetCore.Mvc.DynamicApi
{
    public static class UseEasyCoreDynamicApi
    {
        public static IServiceCollection EasyCoreDynamicApi(this IServiceCollection service)
        {
            service.AddControllers(options =>
            {
                options.Conventions
                .Add(new EasyCoreAspNetCoreMvcDynamicApiControllerName(suffixToTrim: "AppService"));

                options.Conventions
                .Add(new EasyCoreAspNetCoreMvcDynamicApiControllerRoute());
            });

            return service;
        }
    }
}