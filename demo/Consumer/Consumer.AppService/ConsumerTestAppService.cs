using Consumer.AppService.Contracts;
using EasyCore.AspNetCore.Mvc.AppService;
using Provider.AppService.Contracts;

namespace Consumer.AppService
{
    public class ConsumerTestAppService : EasyCoreAppService, IConsumerTestAppService
    {
        private readonly IProviderTestAppService _providerTestAppService;
        private readonly IProviderConsulTestAppService _providerConsulTestAppService;

        public ConsumerTestAppService(
            IProviderTestAppService providerTestAppService,
            IProviderConsulTestAppService providerConsulTestAppService)
        {
            _providerTestAppService = providerTestAppService;
            _providerConsulTestAppService = providerConsulTestAppService;
        }

        public async Task<PostDto> GetRemoteApi()
        {
            return await _providerTestAppService.GetDto();
        }

        public async Task<PostDto> GetRemoteConsulApi()
        {
            return await _providerConsulTestAppService.GetDto();
        }
    }
}
