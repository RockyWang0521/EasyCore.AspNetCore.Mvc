using EasyCore.EFCoreRepository.Repository;
using Provider.EFCore.Entity;

namespace Provider.EFCore.Repository
{
    public class TestEntityRepository : EfCoreRepository<TestDbContext, TestEntity>, ITestEntityRepository
    {
        public TestEntityRepository(TestDbContext dbContext, IServiceProvider serviceProvider)
            : base(dbContext, serviceProvider)
        {
        }
    }
}
