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
            typeof(ILocalRemoteService),
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
    public void EasyCoreRemoteApiClients_Registers_Interface_Only_Proxy_When_Configured()
    {
        var configuration = new TestConfiguration(new Dictionary<string, string?>
        {
            ["RemoteServices:TestService"] = "http://localhost:5094"
        });

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.EasyCoreRemoteApiClients();

        var descriptor = services.Single(d => d.ServiceType == typeof(IConfiguredRemoteService));
        Assert.NotNull(descriptor.ImplementationFactory);
        Assert.Null(descriptor.ImplementationType);
    }

    [Fact]
    public void EasyCoreRemoteApiClients_Skips_Local_Host_Implementation()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new TestConfiguration(new Dictionary<string, string?>()));
        services.AddTransient<IConfiguredRemoteService, ConfiguredRemoteServiceHost>();

        services.EasyCoreRemoteApiClients();

        var descriptor = services.Single(d => d.ServiceType == typeof(IConfiguredRemoteService));
        Assert.Equal(typeof(ConfiguredRemoteServiceHost), descriptor.ImplementationType);
    }

    [ApiHost("TestService")]
    public interface IConfiguredRemoteService : IRemoteAppService
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

    private sealed class ConfiguredRemoteServiceHost : IConfiguredRemoteService
    {
        public Task Ping() => Task.CompletedTask;
    }
}
