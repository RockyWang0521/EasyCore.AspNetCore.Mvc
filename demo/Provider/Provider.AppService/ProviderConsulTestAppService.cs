using EasyCore.AspNetCore.Mvc.AppService;
using Provider.AppService.Contracts;

namespace Provider.AppService
{
    public class ProviderConsulTestAppService : EasyCoreAppService, IProviderConsulTestAppService
    {
        public async Task DeleteDto(int id)
        {
            await Task.CompletedTask;
        }

        public async Task GetContextInfo()
        {
            var currentTenant = CurrentTenant;
            var currentUser = CurrentUser.UserName;
            var guid = GuidFactory.NewGuid;
            var token = CurrentToken.RequestToken;

            await Task.CompletedTask;
        }

        public async Task<PostDto> GetDto()
        {
            await Task.CompletedTask;
            return new PostDto { Id = 1, Title = "Hello World" };
        }

        public async Task PostDto(PostDto dto)
        {
            await Task.CompletedTask;
        }

        public async Task PutDto(PostDto dto)
        {
            await Task.CompletedTask;
        }
    }
}
