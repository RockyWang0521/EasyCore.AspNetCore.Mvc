using EasyCore.AspNetCore.Mvc.RemoteServices;
using EasyCore.Dependency;

namespace Provider.AppService.Contracts
{
    /// <summary>
    /// Sample contract for Dapr sidecar invoke. Enable with EasyCoreRemoteApiDaprClients() on the consumer.
    /// </summary>
    [DaprApp("provider")]
    public interface IProviderDaprTestAppService : IRemoteAppService, ITransientDependency
    {
        Task PostDto(PostDto dto);

        Task<PostDto> GetDto();

        Task PutDto(PostDto dto);

        Task DeleteDto(Guid id);
    }
}
