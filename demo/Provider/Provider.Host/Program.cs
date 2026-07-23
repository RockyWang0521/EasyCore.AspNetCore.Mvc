using EasyCore.AspNetCore.Mvc.AppService;
using EasyCore.AspNetCore.Mvc.DynamicApi;
using EasyCore.AspNetCore.Mvc.RemoteServices;
using EasyCore.Consul;
using EasyCore.Dependency;
using EasyCore.EFCoreRepository;
using EasyCore.Invocation;
using EasyCore.Polly;
using EasyCore.Redis;
using EasyCore.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Provider.AppService.Contracts.Invocations;
using Provider.EFCore;
using Provider.EFCore.Entity;

namespace Provider.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers()
                .AddApplicationPart(typeof(Provider.AppService.ProviderTestAppService).Assembly);
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new()
                {
                    Title = "EasyCore.AspNetCore.Mvc Provider",
                    Version = "v1",
                    Description =
                        "AOP placements A–F: interface type/method, AppService class/method, Controller, combo.\n" +
                        "Start: GET /api/ProviderAopDemoIndex/GetIndexAsync"
                });
            });

            builder.Services.AddDbContext<TestDbContext>();

            builder.Services.AddEasyCoreDynamicApi();
            builder.Services.AddEasyCoreAppServices();
            builder.Services.AddEasyCoreDependency();
            builder.Services.AddEasyCoreEFCoreRepository();
            builder.Services.AddEasyCoreUnitOfWork();
            builder.Services.AddEasyCoreRemoteApiClients();

            // AOP for Dynamic API (MVC filters) — independent packages stack via DI.
            builder.Services.AddEasyCoreInvocation();
            builder.Services.Invocation<AuditInvocation>(ServiceLifetime.Singleton);
            builder.Services.AddEasyCorePolly();
            builder.Services.AddEasyCoreRedis(builder.Configuration.GetSection("EasyCore:Redis"));

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
