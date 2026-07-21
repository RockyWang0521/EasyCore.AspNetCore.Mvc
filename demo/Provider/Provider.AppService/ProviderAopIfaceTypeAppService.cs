using EasyCore.AspNetCore.Mvc.AppService;
using Provider.AppService.Contracts;

namespace Provider.AppService;

/// <summary>Placement A — interface type [Audit]; impl has no attributes.</summary>
public sealed class ProviderAopIfaceTypeAppService : EasyCoreAppService, IProviderAopIfaceTypeAppService
{
    public Task<string> GetPingAsync()
    {
        Console.WriteLine("  [IfaceType] GetPingAsync");
        return Task.FromResult("pong");
    }

    public Task<string> GetEchoAsync(string text)
    {
        Console.WriteLine($"  [IfaceType] GetEchoAsync text={text}");
        return Task.FromResult($"echo:{text}");
    }
}
