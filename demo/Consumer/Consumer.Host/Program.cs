using EasyCore.AspNetCore.Mvc.AppService;
using EasyCore.AspNetCore.Mvc.DynamicApi;
using EasyCore.AspNetCore.Mvc.RemoteServices;
using EasyCore.Consul;
using EasyCore.Dependency;

namespace Consumer.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.EasyCoreDynamicApi();
            builder.Services.EasyCoreAppServices();
            builder.Services.EasyCoreDependency();
            builder.Services.EasyCoreRemoteApiClients();
            builder.Services.EasyCoreRemoteApiConsulClients();
            builder.AddEasyCoreConsul()
                .AddEasyCoreConsulCache()
                .AddEasyCoreConsulLocking()
                .AddEasyCoreConsulServer();
            //builder.Services.EasyCoreRemoteApiK8sClients(options =>
            //{
            //    options.K8sNamespace = "default";
            //    // Final DNS: {K8sDns.ServiceName}.default.svc.cluster.local
            //    options.K8sClusterDomain = "cluster.local";
            //});

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
