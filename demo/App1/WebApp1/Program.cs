using EasyCore.AspNetCore.Mvc.RemoteServices;
using EasyCore.AspNetCore.Mvc.AppService;
using EasyCore.AspNetCore.Mvc.DynamicApi;
using EasyCore.Dependencie;
using EasyCore.Consul;
using App1.EFCore;
using EasyCore.EFCoreRepository;
using Microsoft.EntityFrameworkCore;
using App1.EFCore.Repository;

namespace WebApp1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<TestDbContext>(options =>
            {
                options.UseSqlServer("Server=192.168.157.142;Database=Demo;User Id=sa;Password=Sa123456;TrustServerCertificate=True;Connect Timeout=10;");
            });
            // Use EasyCoreDynamicApi
            builder.Services.EasyCoreDynamicApi();
            // Use EasyCoreAppServices
            builder.Services.EasyCoreAppServices();
            // Use EasyCoreDependencie
            builder.Services.EasyCoreDependencie();
            // Use EasyCoreRemoteApiClients
            builder.Services.EasyCoreRemoteApiClients();
            // Use EasyCoreConsul
            builder.EasyCoreConsul(args).EasyCoreConsulCache().EasyCoreConsulLocking().EasyCoreConsulServer();
            // Use EasyCoreRemoteApiClients
            builder.Services.EasyCoreRemoteApiConsulClients();
            // Use EasyCoreRemoteApiK8sClients
            builder.Services.EasyCoreRemoteApiK8sClients(options =>
            {
                options.K8sNamespace = "default";

                options.K8sClusterDomain = "svc.cluster.local";
            });
            // Use EasyCore EFCore Repository
            builder.Services.EasyCoreEFCoreRepository();

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
