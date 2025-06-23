using App1.EFCore.Entity;
using EasyCore.Dependencie;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App1.AppService.Contracts
{
    public interface IApp1DBAppService : ITransientDependencie
    {
        Task<string> PostEntity(TestEntity Entity);

        Task<List<TestEntity>> GetEntities();
    }
}
