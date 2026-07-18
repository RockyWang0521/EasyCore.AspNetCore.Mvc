using EasyCore.Dependencie;
using Provider.AppService.Contracts;

namespace Consumer.AppService.Contracts
{
    public interface IConsumerTestAppService : ITransientDependencie
    {
        Task<PostDto> GetRemoteApi();

        Task<PostDto> GetRemoteConsulApi();
    }
}
