using EasyCore.AspNetCore.Mvc.RemoteServices;
using EasyCore.Dependency;

namespace Provider.AppService.Contracts
{
    [ConsulService("Provider")]
    public interface IProviderConsulTestAppService : IRemoteAppService, ITransientDependency
    {
        Task PostDto(PostDto dto);

        Task<PostDto> GetDto();

        Task PutDto(PostDto dto);

        Task DeleteDto(Guid id);

        Task GetContextInfo();
    }
}
