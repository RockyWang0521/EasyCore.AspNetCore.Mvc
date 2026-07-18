using EasyCore.AspNetCore.Mvc.RemoteServices;
using EasyCore.Dependencie;

namespace Provider.AppService.Contracts
{
    [ConsulService("Provider")]
    public interface IProviderConsulTestAppService : IRemoteAppService, ITransientDependencie
    {
        Task PostDto(PostDto dto);

        Task<PostDto> GetDto();

        Task PutDto(PostDto dto);

        Task DeleteDto(int id);

        Task EasyCoreAppService();
    }
}
