using EasyCore.AspNetCore.Mvc.AppService;
using EasyCore.AspNetCore.Mvc.DynamicApi;
using EasyCore.Consul;
using EasyCore.Dependencie;

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

            // Use EasyCoreDynamicApi
            builder.Services.EasyCoreDynamicApi();
            // Use EasyCoreAppServices
            builder.Services.EasyCoreAppServices();
            // Use EasyCoreDependencie
            builder.Services.EasyCoreDependencie();
            // Use EasyCoreConsul
            builder.EasyCoreConsul(args).EasyCoreConsulCache().EasyCoreConsulLocking().EasyCoreConsulServer();

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
