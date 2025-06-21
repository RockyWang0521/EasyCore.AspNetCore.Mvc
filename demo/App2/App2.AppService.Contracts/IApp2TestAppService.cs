using EasyCore.Dependencie;

namespace App2.AppService.Contracts
{
    public interface IApp2TestAppService : ITransientDependencie
    {
        Task<Guid> GetTest();
    }
}
