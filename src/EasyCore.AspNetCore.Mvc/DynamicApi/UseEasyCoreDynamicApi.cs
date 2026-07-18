namespace EasyCore.AspNetCore.Mvc.DynamicApi
{
    /// <summary>
    /// Extension methods for enabling EasyCore dynamic API conventions.
    /// </summary>
    public static class UseEasyCoreDynamicApi
    {
        /// <summary>
        /// Registers MVC conventions that trim AppService controller names and map HTTP verbs from method names.
        /// </summary>
        /// <param name="service">The service collection to configure.</param>
        /// <returns>The same <paramref name="service"/> instance for chaining.</returns>
        public static IServiceCollection AddEasyCoreDynamicApi(this IServiceCollection service)
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
