using EasyCore.AspNetCore.Mvc.RemoteServices;
using EasyCore.Dependencie;

namespace Provider.AppService.Contracts
{
    [ApiHost("Provider")]
    public interface IProviderTestAppService : IRemoteAppService, ITransientDependencie
    {
        Task PostDto(PostDto dto);

        Task<PostDto> GetDto();

        Task PutDto(PostDto dto);

        Task DeleteDto(int id);
    }
}
