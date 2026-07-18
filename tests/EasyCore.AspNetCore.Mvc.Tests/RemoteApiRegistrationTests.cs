using EasyCore.AspNetCore.Mvc.RemoteServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EasyCore.AspNetCore.Mvc.Tests;

public class RemoteApiRegistrationTests
{
    [Fact]
    public void ShouldRegisterRemoteProxy_Skips_When_LocalImpl_And_No_RemoteConfig()
    {
        var services = new ServiceCollection();
        services.AddTransient<ILocalRemoteService, LocalRemoteService>();

        var shouldRegister = RemoteApiRegistrationHelper.ShouldRegisterRemoteProxy(
            services,
            typeof(ILocalRemoteService),
            requireRemoteConfig: true,
            hasRemoteConfig: false);

        Assert.False(shouldRegister);
    }

    [Fact]
    public void ShouldRegisterRemoteProxy_Allows_When_No_LocalImpl()
    {
        var services = new ServiceCollection();

        var shouldRegister = RemoteApiRegistrationHelper.ShouldRegisterRemoteProxy(
            services,
            typeof(IProxyOnlyRemoteService),
            requireRemoteConfig: true,
            hasRemoteConfig: true);

        Assert.True(shouldRegister);
    }

    [Fact]
    public void ReplaceWithInterfaceProxy_Removes_Prior_ServiceType_Descriptors()
    {
        var services = new ServiceCollection();
        services.AddTransient<ILocalRemoteService, LocalRemoteService>();

        RemoteApiRegistrationHelper.ReplaceWithInterfaceProxy(
            services,
            typeof(ILocalRemoteService),
            _ => new object());

        var descriptors = services.Where(d => d.ServiceType == typeof(ILocalRemoteService)).ToList();
        Assert.Single(descriptors);
        Assert.NotNull(descriptors[0].ImplementationFactory);
        Assert.Null(descriptors[0].ImplementationType);
    }

    [Fact]
    public void EasyCoreRemoteApiClients_Registers_Interface_Only_Proxy_When_No_Local_Implementation()
    {
        var configuration = new TestConfiguration(new Dictionary<string, string?>
        {
            ["RemoteServices:ProxyOnly"] = "http://localhost:5094"
        });

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.EasyCoreRemoteApiClients();

        var descriptor = services.Single(d => d.ServiceType == typeof(IProxyOnlyRemoteService));
        Assert.NotNull(descriptor.ImplementationFactory);
        Assert.Null(descriptor.ImplementationType);
    }

    [Fact]
    public void EasyCoreRemoteApiClients_Skips_Local_Host_Implementation()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new TestConfiguration(new Dictionary<string, string?>()));
        services.AddTransient<IHostedRemoteService, HostedRemoteService>();

        services.EasyCoreRemoteApiClients();

        var descriptor = services.Single(d => d.ServiceType == typeof(IHostedRemoteService));
        Assert.Equal(typeof(HostedRemoteService), descriptor.ImplementationType);
    }

    [Fact]
    public void HasLocalImplementation_Detects_Concrete_Class_In_Loaded_Assemblies()
    {
        var services = new ServiceCollection();

        Assert.True(RemoteApiRegistrationHelper.HasLocalImplementation(services, typeof(IHostedRemoteService)));
        Assert.False(RemoteApiRegistrationHelper.HasLocalImplementation(services, typeof(IProxyOnlyRemoteService)));
    }

    /// <summary>
    /// Remote-only contract with no concrete class in this assembly (consumer-side scenario).
    /// </summary>
    [ApiHost("ProxyOnly")]
    public interface IProxyOnlyRemoteService : IRemoteAppService
    {
        Task Ping();
    }

    /// <summary>
    /// Host-side contract that has a local implementation in this assembly.
    /// </summary>
    [ApiHost("HostedService")]
    public interface IHostedRemoteService : IRemoteAppService
    {
        Task Ping();
    }

    public interface ILocalRemoteService : IRemoteAppService
    {
        Task Ping();
    }

    private sealed class LocalRemoteService : ILocalRemoteService
    {
        public Task Ping() => Task.CompletedTask;
    }

    private sealed class HostedRemoteService : IHostedRemoteService
    {
        public Task Ping() => Task.CompletedTask;
    }
}
