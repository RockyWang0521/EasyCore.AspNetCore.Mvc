using EasyCore.Dependencie;

namespace App1.AppService.Contracts
{
    public interface IApp1DBAppService : ITransientDependencie
    {
        Task PostEntity(TestEntityDto Entity);

        Task<List<TestEntityDto>> GetEntities();
    }
}
