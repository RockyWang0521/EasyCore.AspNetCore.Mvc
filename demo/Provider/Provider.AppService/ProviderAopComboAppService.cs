using EasyCore.AspNetCore.Mvc.AppService;
using Provider.AppService.Contracts;

namespace Provider.AppService;

/// <summary>Placement C — combo on interface (Audit + Polly + ServerCache).</summary>
public sealed class ProviderAopComboAppService : EasyCoreAppService, IProviderAopComboAppService
{
    private static int _unstableAttempts;
    private static int _stackedAttempts;

    public Task<string> GetUnstableAsync()
    {
        var n = Interlocked.Increment(ref _unstableAttempts);
        Console.WriteLine($"  [Combo] GetUnstableAsync attempt={n}");
        if (n % 3 != 0)
        {
            throw new InvalidOperationException($"Transient failure #{n}");
        }

        return Task.FromResult($"combo-unstable-ok-{n}");
    }

    public async Task<string> GetCachedAsync(string key)
    {
        Console.WriteLine($"  [Combo] GetCachedAsync key={key} (body)");
        await Task.Delay(30).ConfigureAwait(false);
        return $"combo-cache:{key}:{DateTime.UtcNow:O}";
    }

    public Task<string> GetStackedAsync(string key)
    {
        var n = Interlocked.Increment(ref _stackedAttempts);
        Console.WriteLine($"  [Combo] GetStackedAsync key={key} attempt={n}");
        if (n == 1)
        {
            throw new InvalidOperationException("First stacked call fails (Consumer retry may recover).");
        }

        return Task.FromResult($"combo-stacked:{key}:{n}");
    }
}
