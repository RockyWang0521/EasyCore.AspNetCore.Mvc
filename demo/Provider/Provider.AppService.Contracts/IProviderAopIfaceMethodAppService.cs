using EasyCore.AspNetCore.Mvc.RemoteServices;
using EasyCore.Dependency;
using EasyCore.Polly;
using EasyCore.Redis.Service.Attribute;

namespace Provider.AppService.Contracts;

/// <summary>
/// Placement B — interface method: attributes only on marked methods.
/// GetCachedAsync / GetUnstableAsync instrumented; GetPlainAsync is not.
/// </summary>
[ApiHost("Provider")]
public interface IProviderAopIfaceMethodAppService : IRemoteAppService, ITransientDependency
{
    [ServerCache(CacheSeconds = 60)]
    Task<string> GetCachedAsync(string key);

    [PollyConfig(MaxRetry = 3, RetryIntervalSeconds = 0)]
    Task<string> GetUnstableAsync();

    Task<string> GetPlainAsync(string key);
}
