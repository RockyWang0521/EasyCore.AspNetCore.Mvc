using EasyCore.AspNetCore.Mvc.RemoteServices;
using EasyCore.Dependencie;

namespace App1.AppService.Contracts
{
    [ApiHost("Service1")]
    public interface IApp1TestAppService : IRemoteAppService, ITransientDependencie
    {
        Task<Guid> GetGuid();

        Task<PostDto> PostDto(PostDto dto);

        Task GetGuid1();

        void GetGuid2();
    }
}
