using App1.AppService.Contracts;
using EasyCore.AspNetCore.Mvc.AppService;

namespace App1.AppService
{
    public class App1TestAppService : EasyCoreAppService, IApp1TestAppService
    {
        public async Task DeleteDto(int id)
        {
            // do something

            await Task.CompletedTask;
        }

        public async Task<PostDto> GetDto()
        {
            await Task.CompletedTask;
            return new PostDto { Id = 1, Title = "Hello World" };
        }

        public async Task PostDto(PostDto dto)
        {
            // do something

            await Task.CompletedTask;
        }

        public async Task PutDto(PostDto dto)
        {
            // do something

            await Task.CompletedTask;
        }
    }
}
