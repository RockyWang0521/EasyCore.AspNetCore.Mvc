using EasyCore.AspNetCore.Mvc.AppService;
using Provider.AppService.Contracts;

namespace Provider.AppService
{
    public class ProviderTestAppService : EasyCoreAppService, IProviderTestAppService
    {
        public async Task DeleteDto(int id)
        {
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
