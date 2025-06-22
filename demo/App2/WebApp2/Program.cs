using EasyCore.AspNetCore.Mvc.RemoteServices;
using EasyCore.AspNetCore.Mvc.AppService;
using EasyCore.AspNetCore.Mvc.DynamicApi;
using EasyCore.Dependencie;
using EasyCore.Consul;

namespace WebApp2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Use EasyCoreDynamicApi
            builder.Services.EasyCoreDynamicApi();
            // Use EasyCoreAppServices
            builder.Services.EasyCoreAppServices();
            // Use EasyCoreRemoteApiClients
            builder.Services.EasyCoreRemoteApiClients();
            // Use EasyCoreDependencie
            builder.Services.EasyCoreDependencie();
            // Use EasyCoreConsul
            builder.EasyCoreConsul(args).EasyCoreConsulCache().EasyCoreConsulLocking().EasyCoreConsulServer();
            // Use EasyCoreRemoteApiClients
            builder.Services.EasyCoreRemoteApiConsulClients();
            // Use EasyCoreRemoteApiClients
            builder.Services.EasyCoreRemoteApiK8sClients(options =>
            {
                options.K8sNamespace = "default";

                options.K8sClusterDomain = "svc.cluster.local";
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Use EasyCoreConsul
            app.UseEasyCoreConsul();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
