using EasyCore.EFCoreRepository.IRepository;
using EasyCore.Dependencie;
using App1.EFCore.Entity;

namespace App1.EFCore.Repository
{
    public interface ITestEntityRepository : IRepository<TestEntity>, ITransientDependencie
    {

    }
}
