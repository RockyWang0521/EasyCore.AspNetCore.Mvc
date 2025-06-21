using App1.AppService.Contracts;
using EasyCore.AspNetCore.Mvc.AppService;

namespace App1.AppService
{
    public class App1TestAppService : EasyCoreAppService, IApp1TestAppService
    {
        public App1TestAppService() { }

        public async Task<Guid> GetGuid()
        {
            await Task.CompletedTask;

            var token = CurrentToken.RequestToken;

            return GuidFactory.NewGuid;
        }

        public async Task GetGuid1()
        {
            await Task.CompletedTask;
        }

        public void GetGuid2()
        {

        }

        public async Task<PostDto> PostDto(PostDto dto)
        {
            await Task.CompletedTask;

            return new PostDto
            {
                Id = dto.Id,
                Title = dto.Title,
                Content = dto.Content
            };
        }
    }
}
