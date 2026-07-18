using EasyCore.Dependency;
using EasyCore.EFCoreRepository.IRepository;
using Provider.EFCore.Entity;

namespace Provider.EFCore.Repository
{
    public interface ITestEntityRepository : IRepository<TestDbContext, TestEntity>, ITransientDependency
    {
    }
}
