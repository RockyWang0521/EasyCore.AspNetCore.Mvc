using EasyCore.Dependency;
using Provider.AppService.Contracts;

namespace Consumer.AppService.Contracts
{
    public interface IConsumerTestAppService : ITransientDependency
    {
        Task<PostDto> GetRemoteApi();

        Task<PostDto> GetRemoteConsulApi();
    }
}
