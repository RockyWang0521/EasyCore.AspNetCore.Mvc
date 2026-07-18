using EasyCore.AspNetCore.Mvc.AppService;
using EasyCore.AspNetCore.Mvc.DynamicApi;
using EasyCore.AspNetCore.Mvc.RemoteServices;
using EasyCore.Dependencie;
using EasyCore.EFCoreRepository;
using Provider.EFCore;

namespace Provider.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<TestDbContext>();

            builder.Services.EasyCoreDynamicApi();
            builder.Services.EasyCoreAppServices();
            builder.Services.EasyCoreDependencie();
            builder.Services.EasyCoreEFCoreRepository();
            builder.Services.EasyCoreRemoteApiClients();
            // builder.Services.EasyCoreRemoteApiConsulClients();
            //builder.Services.EasyCoreRemoteApiK8sClients(options =>
            //{
            //    options.K8sNamespace = "default";
            //    options.K8sClusterDomain = "svc.cluster.local";
            //});

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
