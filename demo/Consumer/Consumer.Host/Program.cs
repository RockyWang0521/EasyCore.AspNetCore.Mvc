using EasyCore.AspNetCore.Mvc.AppService;
using EasyCore.AspNetCore.Mvc.DynamicApi;
using EasyCore.AspNetCore.Mvc.RemoteServices;
using EasyCore.Consul;
using EasyCore.Dependency;
using EasyCore.Invocation;
using EasyCore.Polly;
using EasyCore.Redis;
using Provider.AppService.Contracts.Invocations;

namespace Consumer.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new()
                {
                    Title = "EasyCore.AspNetCore.Mvc Consumer",
                    Version = "v1",
                    Description =
                        "Remote DispatchProxy + Castle AOP for interface placements (A/B/C).\n" +
                        "Start Provider :5094 first. Index on Provider: GET /api/ProviderAopDemoIndex/GetIndexAsync\n" +
                        "Consumer: GET /api/ConsumerTest/GetAopIfaceTypePingAsync, GetAopComboCachedAsync?key=demo, …"
                });
            });

            builder.Services.AddEasyCoreDynamicApi();
            builder.Services.AddEasyCoreAppServices();
            builder.Services.AddEasyCoreDependency();

            // Interceptors stack onto remote DispatchProxy via RemoteInterceptorComposer.
            builder.Services.AddEasyCoreInvocation();
            builder.Services.Invocation<AuditInvocation>(ServiceLifetime.Singleton);
            builder.Services.AddEasyCorePolly();
            builder.Services.AddEasyCoreRedis(builder.Configuration.GetSection("EasyCore:Redis"));

            builder.Services.AddEasyCoreRemoteApiClients();
            builder.Services.AddEasyCoreRemoteApiConsulClients();
            builder.AddEasyCoreConsul()
                .AddEasyCoreConsulCache()
                .AddEasyCoreConsulLocking()
                .AddEasyCoreConsulServer();
            //builder.Services.AddEasyCoreRemoteApiK8sClients(options =>
            //{
            //    options.K8sNamespace = "default";
            //    // Final DNS: {K8sDns.ServiceName}.default.svc.cluster.local
            //    options.K8sClusterDomain = "cluster.local";
            //});
            //builder.Services.AddEasyCoreRemoteApiNacosClients(); // needs Nacos:ServerAddresses
            //builder.Services.AddEasyCoreRemoteApiDaprClients();  // needs Dapr sidecar / Dapr:HttpEndpoint

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseEasyCoreConsul();

            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
