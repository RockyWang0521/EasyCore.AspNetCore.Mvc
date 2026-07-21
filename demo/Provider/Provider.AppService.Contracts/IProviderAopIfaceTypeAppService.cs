using EasyCore.AspNetCore.Mvc.RemoteServices;
using EasyCore.Dependency;
using Provider.AppService.Contracts.Attributes;

namespace Provider.AppService.Contracts;

/// <summary>
/// Placement A — interface type: [Audit] on the contract applies to every method.
/// Provider: MVC convention → Filter. Consumer: Castle over DispatchProxy.
/// </summary>
[ApiHost("Provider")]
[Audit]
public interface IProviderAopIfaceTypeAppService : IRemoteAppService, ITransientDependency
{
    Task<string> GetPingAsync();

    Task<string> GetEchoAsync(string text);
}
