using App1.AppService.Contracts;
using EasyCore.AspNetCore.Mvc.AppService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App1.AppService
{
    public class App1ConsulTestAppService : EasyCoreAppService, IApp1ConsulTestAppService
    {
        public async Task<Guid> GetGuid()
        {
            await Task.CompletedTask;

            return GuidFactory.NewGuid;
        }
    }
}
