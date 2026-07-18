using EasyCore.AspNetCore.Mvc.RemoteServices;
using EasyCore.Dependency;

namespace Provider.AppService.Contracts
{
    /// <summary>
    /// Sample contract for Nacos discovery. Enable with EasyCoreRemoteApiNacosClients() on the consumer.
    /// </summary>
    [NacosService("Provider")]
    public interface IProviderNacosTestAppService : IRemoteAppService, ITransientDependency
    {
        Task PostDto(PostDto dto);

        Task<PostDto> GetDto();

        Task PutDto(PostDto dto);

        Task DeleteDto(Guid id);
    }
}
