using EasyCore.AspNetCore.Mvc.RemoteServices;
using EasyCore.Dependency;
using EasyCore.Polly;
using EasyCore.Redis.Service.Attribute;
using Provider.AppService.Contracts.Attributes;

namespace Provider.AppService.Contracts;

/// <summary>
/// Placement C — stacked on interface methods: Audit (type) + Polly + ServerCache.
/// Best path for Consumer remote AOP (attributes travel with the contract).
/// </summary>
[ApiHost("Provider")]
[Audit]
public interface IProviderAopComboAppService : IRemoteAppService, ITransientDependency
{
    [PollyConfig(MaxRetry = 3, RetryIntervalSeconds = 0)]
    Task<string> GetUnstableAsync();

    [ServerCache(CacheSeconds = 60)]
    Task<string> GetCachedAsync(string key);

    [PollyConfig(MaxRetry = 2, RetryIntervalSeconds = 0)]
    [ServerCache(CacheSeconds = 30)]
    Task<string> GetStackedAsync(string key);
}
