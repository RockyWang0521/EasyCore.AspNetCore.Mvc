using EasyCore.Dependency;

namespace Provider.AppService.Contracts;

/// <summary>
/// Placement D/E — plain contract; AOP lives on AppService class / methods (Provider Filter only).
/// Not a remote contract — Consumer cannot see class/method attributes.
/// </summary>
public interface IProviderAopImplPlacementAppService : ITransientDependency
{
    Task<string> GetFromClassAsync(string name);

    Task<string> GetFromMethodCachedAsync(string key);

    Task<string> GetFromMethodUnstableAsync();

    Task<string> GetPlainAsync(string name);
}
