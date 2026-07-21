using EasyCore.AspNetCore.Mvc.AppService;
using EasyCore.Polly;
using EasyCore.Redis.Service.Attribute;
using Provider.AppService.Contracts;
using Provider.AppService.Contracts.Attributes;

namespace Provider.AppService;

/// <summary>
/// Placement D — AppService class [Audit] (API / Dynamic API class-level).
/// Placement E — method-level [ServerCache] / [PollyConfig] on this class.
/// Provider Filter only; not remoted.
/// </summary>
[Audit]
public sealed class ProviderAopImplPlacementAppService : EasyCoreAppService, IProviderAopImplPlacementAppService
{
    private static int _unstableAttempts;

    public Task<string> GetFromClassAsync(string name)
    {
        Console.WriteLine($"  [ImplClass] GetFromClassAsync name={name} (class [Audit])");
        return Task.FromResult($"class-audit:{name}");
    }

    [ServerCache(CacheSeconds = 60)]
    public async Task<string> GetFromMethodCachedAsync(string key)
    {
        Console.WriteLine($"  [ImplMethod] GetFromMethodCachedAsync key={key} (body)");
        await Task.Delay(30).ConfigureAwait(false);
        return $"impl-method-cache:{key}:{DateTime.UtcNow:O}";
    }

    [PollyConfig(MaxRetry = 3, RetryIntervalSeconds = 0)]
    public Task<string> GetFromMethodUnstableAsync()
    {
        var n = Interlocked.Increment(ref _unstableAttempts);
        Console.WriteLine($"  [ImplMethod] GetFromMethodUnstableAsync attempt={n} (Filter: no retry)");
        if (n % 3 != 0)
        {
            throw new InvalidOperationException($"Transient failure #{n}");
        }

        return Task.FromResult($"impl-method-ok-{n}");
    }

    public Task<string> GetPlainAsync(string name)
    {
        Console.WriteLine($"  [ImplClass] GetPlainAsync name={name} (class [Audit] only)");
        return Task.FromResult($"plain:{name}");
    }
}
