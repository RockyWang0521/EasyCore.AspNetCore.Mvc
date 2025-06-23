using App1.AppService.Contracts;
using App1.EFCore.Entity;
using EasyCore.AspNetCore.Mvc.AppService;

namespace App1.AppService
{
    public class App1DBAppService : EasyCoreAppService, IApp1DBAppService
    {
        public async Task<List<TestEntity>> GetEntities()
        {

        }

        public async Task<string> PostEntity(TestEntity Entity)
        {

        }
    }
}
