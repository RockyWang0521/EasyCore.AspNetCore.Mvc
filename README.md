# EasyCore.AspNetCore.Mvc

EasyCore.AspNetCore.Mvc 支持动态Api注册，支持远端HttpProxy动态服务访问。

一、Dynamic Api

1.注册Dynamic Api

```
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Use EasyCoreDynamicApi
            builder.Services.EasyCoreDynamicApi();
            // Use EasyCoreDependencie
            builder.Services.EasyCoreDependencie();

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
```

2.继承EasyCoreAppService

```
 public class App1ConsulTestAppService : EasyCoreAppService, IApp1ConsulTestAppService
 {
     public async Task DeleteDto(int id)
     {
         // do something

         await Task.CompletedTask;
     }

     public async Task<PostDto> GetDto()
     {
         await Task.CompletedTask;
         return new PostDto { Id = 1, Title = "Hello World" };
     }

     public async Task PostDto(PostDto dto)
     {
         // do something

         await Task.CompletedTask;
     }

     public async Task PutDto(PostDto dto)
     {
         // do something

         await Task.CompletedTask;
     }
 }
```
![输入图片说明](https://foruda.gitee.com/images/1750742306209061072/17b4572f_10509988.png "捕获.PNG")

AppService就会自动变成一个Api控制器，并在Swagger界面中注册并显示。

二、远端服务动态代理

如果http请求带有Token时，动态代理访问时会自动带上Token访问远端服务，用户无需担心。

1.注册远端服务动态代理。

EasyCore.AspNetCore.Mvc 提供三种特性，并支持三种远端服务动态代理调用分别为：ApiHost；Consul；K8S。

1.1 ApiHost

1.1.1 远端项目(另外的项目)注册RemoteApiClients

```
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
     // Use EasyCoreRemoteApiClients
     builder.Services.EasyCoreRemoteApiClients();
    
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
```
1.1.2 本地项目接口继承允许远端访问

允许被远端服务调用的接口需要继承IRemoteAppService。

```
 [ApiHost("Service1")]
 public interface IApp1TestAppService : IRemoteAppService, ITransientDependencie
 {
     Task PostDto(PostDto dto);

     Task<PostDto> GetDto();

     Task PutDto(PostDto dto);

     Task DeleteDto(int id);
 }
```
[ApiHost("Service1")]中输入服务的名字，远端服务项目appsettings.json中配置ip地址。


```
  "RemoteServices": {
    "Service1": "http://localhost:5094"
  },
```


1.1.3 远端项目可以直接以抽象依赖注入的方式访问

```
   public class App2TestAppService : EasyCoreAppService, IApp2TestAppService
   {
       private readonly IApp1TestAppService _app1TestAppService;
       private readonly IApp1ConsulTestAppService _app1ConsulTestAppService;

       public App2TestAppService(IApp1TestAppService app1TestAppService, IApp1ConsulTestAppService app1ConsulTestAppService)
       {
           _app1TestAppService = app1TestAppService;
           _app1ConsulTestAppService = app1ConsulTestAppService;
       }

       public async Task<PostDto> GetRemoteApi()
       {
           return await _app1TestAppService.GetDto();
       }

       public async Task<PostDto> GetRemoteConsulApi()
       {
           return await _app1ConsulTestAppService.GetDto();
       }
   }
```
这样远端项目就可以直接可以HttpProxy动态代理访问Api。

1.2 ConsulService

1.2.1 远端项目(另外的项目)注册RemoteApiConsulClients

```
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
       // Use EasyCoreRemoteApiClients
       builder.Services.EasyCoreRemoteApiClients();
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
```
可以使用EasyCoreConsul来实现服务注册至Consul中。

1.2.2 接口继承

继承IRemoteAppService允许远端访问接口。

```
  [ConsulService("App1")]
  public interface IApp1ConsulTestAppService : IRemoteAppService, ITransientDependencie
  {
      Task PostDto(PostDto dto);

      Task<PostDto> GetDto();

      Task PutDto(PostDto dto);

      Task DeleteDto(int id);
  }
```
  [ConsulService("App1")] 特性中输入Consul注册的服务名即可进行远端调用。

1.3 K8S

1.3.1 远端项目(另外的项目)注册RemoteApiK8sClients

K8S注册远端调用时，需要填写Namespace命名控件名字和ClusterDomain根域名。

```
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
       // Use EasyCoreRemoteApiK8sClients
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

       app.UseAuthorization();

       app.MapControllers();

       app.Run();
   }
```

  [K8sDns("Service1")] 输入服务名即可。

```
  [K8sDns("Service1")]
  public interface IApp1TestAppService : IRemoteAppService, ITransientDependencie
  {
      Task PostDto(PostDto dto);

      Task<PostDto> GetDto();
  }
```


三、EasyCoreAppService

EasyCoreAppService提供了多种快捷使用接口。

1. 当前用户。

2. 有序的Guid。

3. 当前租户。

4.http请求中的Token


```
    public async Task EasyCoreAppService()
    {
        var currentTenant = CurrentTenant;

        var currentUser = CurrentUser.UserName;

        var guid = GuidFactory.NewGuid;

        var token = CurrentToken.RequestToken;

        await Task.CompletedTask;
    }
```
