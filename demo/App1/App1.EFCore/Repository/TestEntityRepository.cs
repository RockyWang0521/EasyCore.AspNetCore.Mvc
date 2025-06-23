using App1.EFCore.Entity;
using EasyCore.EFCoreRepository.Repository;

namespace App1.EFCore.Repository
{
    public class TestEntityRepository : EfCoreRepository<TestDbContext, TestEntity>, ITestEntityRepository
    {
        public TestEntityRepository(TestDbContext dbContext, IServiceProvider serviceProvider) : base(dbContext, serviceProvider)
        {

        }
    }
}
