using EasyCore.AspNetCore.Mvc.AppService;
using Provider.AppService.Contracts;

namespace Provider.AppService;

/// <summary>Placement B — interface method attrs; GetPlainAsync has none.</summary>
public sealed class ProviderAopIfaceMethodAppService : EasyCoreAppService, IProviderAopIfaceMethodAppService
{
    private static int _unstableAttempts;

    public async Task<string> GetCachedAsync(string key)
    {
        Console.WriteLine($"  [IfaceMethod] GetCachedAsync key={key} (body)");
        await Task.Delay(30).ConfigureAwait(false);
        return $"iface-method-cache:{key}:{DateTime.UtcNow:O}";
    }

    public Task<string> GetUnstableAsync()
    {
        var n = Interlocked.Increment(ref _unstableAttempts);
        Console.WriteLine($"  [IfaceMethod] GetUnstableAsync attempt={n}");
        if (n % 3 != 0)
        {
            throw new InvalidOperationException($"Transient failure #{n}");
        }

        return Task.FromResult($"iface-method-ok-{n}");
    }

    public Task<string> GetPlainAsync(string key)
    {
        Console.WriteLine($"  [IfaceMethod] GetPlainAsync key={key} (no AOP expected)");
        return Task.FromResult($"plain:{key}");
    }
}
