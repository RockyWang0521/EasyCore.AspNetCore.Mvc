using EasyCore.AspNetCore.Mvc.RemoteServices;
using EasyCore.Dependencie;

namespace App1.AppService.Contracts
{
    [ApiHost("Service1")]
    public interface IApp1TestAppService : IRemoteAppService, ITransientDependencie
    {
        Task PostDto(PostDto dto);

        Task<PostDto> GetDto();

        Task PutDto(PostDto dto);

        Task DeleteDto(int id);
    }
}
