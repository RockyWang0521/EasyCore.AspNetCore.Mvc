using App1.AppService.Contracts;
using App1.EFCore.Entity;
using App1.EFCore.Repository;
using EasyCore.AspNetCore.Mvc.AppService;

namespace App1.AppService
{
    public class App1DBAppService : EasyCoreAppService, IApp1DBAppService
    {
        private readonly ITestEntityRepository _testEntityRepository;

        public App1DBAppService(ITestEntityRepository testEntityRepository) => _testEntityRepository = testEntityRepository;

        public async Task<List<TestEntityDto>> GetEntities()
        {
            var entities = await _testEntityRepository.GetListAsync();

            return entities.Select(entity => new TestEntityDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Age = entity.Age
            }).ToList();
        }

        public async Task PostEntity(TestEntityDto Entity)
        {
            var entity = new TestEntity
            {
                Id = GuidFactory.NewGuid,
                Name = Entity.Name,
                Age = Entity.Age
            };

            await _testEntityRepository.InsertAsync(entity);
        }
    }
}
