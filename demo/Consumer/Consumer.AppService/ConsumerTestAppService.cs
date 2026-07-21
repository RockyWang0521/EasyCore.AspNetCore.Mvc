using Consumer.AppService.Contracts;
using EasyCore.AspNetCore.Mvc.AppService;
using Provider.AppService.Contracts;

namespace Consumer.AppService
{
    public class ConsumerTestAppService : EasyCoreAppService, IConsumerTestAppService
    {
        private readonly IProviderTestAppService _providerTestAppService;
        private readonly IProviderConsulTestAppService _providerConsulTestAppService;
        private readonly IProviderAopIfaceTypeAppService _ifaceType;
        private readonly IProviderAopIfaceMethodAppService _ifaceMethod;
        private readonly IProviderAopComboAppService _combo;

        public ConsumerTestAppService(
            IProviderTestAppService providerTestAppService,
            IProviderConsulTestAppService providerConsulTestAppService,
            IProviderAopIfaceTypeAppService ifaceType,
            IProviderAopIfaceMethodAppService ifaceMethod,
            IProviderAopComboAppService combo)
        {
            _providerTestAppService = providerTestAppService;
            _providerConsulTestAppService = providerConsulTestAppService;
            _ifaceType = ifaceType;
            _ifaceMethod = ifaceMethod;
            _combo = combo;
        }

        public Task<PostDto> GetRemoteApi()
            => _providerTestAppService.GetDto();

        public Task<PostDto> GetRemoteConsulApi()
            => _providerConsulTestAppService.GetDto();

        public Task<string> GetAopIfaceTypePingAsync()
            => _ifaceType.GetPingAsync();

        public Task<string> GetAopIfaceTypeEchoAsync(string text)
            => _ifaceType.GetEchoAsync(text);

        public Task<string> GetAopIfaceMethodCachedAsync(string key)
            => _ifaceMethod.GetCachedAsync(key);

        public Task<string> GetAopIfaceMethodUnstableAsync()
            => _ifaceMethod.GetUnstableAsync();

        public Task<string> GetAopIfaceMethodPlainAsync(string key)
            => _ifaceMethod.GetPlainAsync(key);

        public Task<string> GetAopComboUnstableAsync()
            => _combo.GetUnstableAsync();

        public Task<string> GetAopComboCachedAsync(string key)
            => _combo.GetCachedAsync(key);

        public Task<string> GetAopComboStackedAsync(string key)
            => _combo.GetStackedAsync(key);
    }
}
