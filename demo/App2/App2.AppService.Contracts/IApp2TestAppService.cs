using App1.AppService.Contracts;
using EasyCore.Dependencie;

namespace App2.AppService.Contracts
{
    public interface IApp2TestAppService : ITransientDependencie
    {
        Task<PostDto> GetRemoteApi();

        Task<PostDto> GetRemoteConsulApi();
    }
}
