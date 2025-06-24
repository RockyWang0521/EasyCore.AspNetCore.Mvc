using App1.AppService.Contracts;
using App2.AppService.Contracts;
using EasyCore.AspNetCore.Mvc.AppService;

namespace App2.AppService
{
    public class App2TestAppService : EasyCoreAppService, IApp2TestAppService
    {
        private readonly IApp1TestAppService _app1TestAppService;
        private readonly IApp1ConsulTestAppService _app1ConsulTestAppService;

        public App2TestAppService(IApp1TestAppService app1TestAppService, IApp1ConsulTestAppService app1ConsulTestAppService)
        {
            _app1TestAppService = app1TestAppService;
            _app1ConsulTestAppService = app1ConsulTestAppService;
        }

        public async Task<PostDto> GetRemoteApi()
        {
            return await _app1TestAppService.GetDto();
        }

        public async Task<PostDto> GetRemoteConsulApi()
        {
            return await _app1ConsulTestAppService.GetDto();
        }
    }
}
