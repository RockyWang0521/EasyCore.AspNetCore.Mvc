using EasyCore.AspNetCore.Mvc.AppService;
using EasyCore.AspNetCore.Mvc.DynamicApi;
using EasyCore.AspNetCore.Mvc.RemoteServices;
using EasyCore.Consul;
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

            // Register this Provider instance into Consul (ServiceName=Provider).
            builder.EasyCoreConsul(args).EasyCoreConsulCache().EasyCoreConsulLocking().EasyCoreConsulServer();

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
