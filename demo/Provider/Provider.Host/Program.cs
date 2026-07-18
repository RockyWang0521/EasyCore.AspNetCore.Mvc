using EasyCore.AspNetCore.Mvc.AppService;
using EasyCore.AspNetCore.Mvc.DynamicApi;
using EasyCore.AspNetCore.Mvc.RemoteServices;
using EasyCore.Consul;
using EasyCore.Dependency;
using EasyCore.EFCoreRepository;
using Microsoft.EntityFrameworkCore;
using Provider.EFCore;
using Provider.EFCore.Entity;

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
            builder.Services.EasyCoreDependency();
            builder.Services.EasyCoreEFCoreRepository();
            builder.Services.EasyCoreRemoteApiClients();

            // Register this Provider instance into Consul (ServiceName=Provider).
            builder.AddEasyCoreConsul()
                .AddEasyCoreConsulCache()
                .AddEasyCoreConsulLocking()
                .AddEasyCoreConsulServer();

            var app = builder.Build();

            InitializeDatabase(app);

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

        private static void InitializeDatabase(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TestDbContext>();
            db.Database.Migrate();

            if (!db.TestEntity.Any())
            {
                db.TestEntity.Add(new TestEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Hello World",
                    Age = 18,
                    CreateTime = DateTime.Now,
                    ConcurrencyStamp = Guid.NewGuid().ToString("N")
                });
                db.SaveChanges();
            }
        }
    }
}
