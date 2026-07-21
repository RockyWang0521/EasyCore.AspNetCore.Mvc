using EasyCore.Dependency;
using Provider.AppService.Contracts;

namespace Consumer.AppService.Contracts
{
    public interface IConsumerTestAppService : ITransientDependency
    {
        Task<PostDto> GetRemoteApi();

        Task<PostDto> GetRemoteConsulApi();

        // A — interface type remote
        Task<string> GetAopIfaceTypePingAsync();

        Task<string> GetAopIfaceTypeEchoAsync(string text);

        // B — interface method remote
        Task<string> GetAopIfaceMethodCachedAsync(string key);

        Task<string> GetAopIfaceMethodUnstableAsync();

        Task<string> GetAopIfaceMethodPlainAsync(string key);

        // C — combo remote (DispatchProxy + Castle)
        Task<string> GetAopComboUnstableAsync();

        Task<string> GetAopComboCachedAsync(string key);

        Task<string> GetAopComboStackedAsync(string key);
    }
}
