using App1.AppService.Contracts;
using EasyCore.AspNetCore.Mvc.AppService;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace App1.AppService
{
    public class App1TestAppService : EasyCoreAppService, IApp1TestAppService
    {
        private readonly ILogger<App1TestAppService> _logger;

        public App1TestAppService(ILogger<App1TestAppService> logger) => _logger = logger;

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

        public async Task GetGuids1()
        {
            Stopwatch sw = new Stopwatch();

            sw.Start();

            for (int i = 0; i < 10000; i++)
            {
                Console.WriteLine($"{GuidFactory.NewGuid}");
            }

            sw.Stop();

            _logger.LogInformation($"GetGuids1: {sw.ElapsedMilliseconds} ms");

            await Task.CompletedTask;
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
