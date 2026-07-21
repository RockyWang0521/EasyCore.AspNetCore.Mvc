using EasyCore.Polly;
using EasyCore.Redis.Service.Attribute;
using Microsoft.AspNetCore.Mvc;
using Provider.AppService.Contracts.Attributes;

namespace Provider.Host.Controllers;

/// <summary>
/// Placement F — classic MVC Controller / Action (API) via IFilterFactory.
/// </summary>
[ApiController]
[Route("api/aop-controller")]
[Audit]
public sealed class ProviderAopPlacementController : ControllerBase
{
    private static int _attempts;

    [HttpGet("ping")]
    public Task<string> GetPing()
    {
        Console.WriteLine("  [Controller] GetPing (class [Audit])");
        return Task.FromResult("controller-pong");
    }

    [HttpGet("cached")]
    [ServerCache(CacheSeconds = 60)]
    public async Task<string> GetCached([FromQuery] string key = "demo")
    {
        Console.WriteLine($"  [Controller] GetCached key={key} (body)");
        await Task.Delay(30).ConfigureAwait(false);
        return $"controller-cache:{key}:{DateTime.UtcNow:O}";
    }

    [HttpGet("unstable")]
    [PollyConfig(MaxRetry = 3, RetryIntervalSeconds = 0)]
    public Task<string> GetUnstable()
    {
        var n = Interlocked.Increment(ref _attempts);
        Console.WriteLine($"  [Controller] GetUnstable attempt={n}");
        if (n % 3 != 0)
        {
            throw new InvalidOperationException($"Transient failure #{n}");
        }

        return Task.FromResult($"controller-ok-{n}");
    }
}
