using EasyCore.AspNetCore.Mvc.AppService;
using Provider.AppService.Contracts;
using Provider.EFCore.Entity;
using Provider.EFCore.Repository;

namespace Provider.AppService
{
    public class ProviderConsulTestAppService : EasyCoreAppService, IProviderConsulTestAppService
    {
        private readonly ITestEntityRepository _repository;

        public ProviderConsulTestAppService(ITestEntityRepository repository)
        {
            _repository = repository;
        }

        public async Task DeleteDto(Guid id)
        {
            await _repository.DeleteAsync(e => e.Id == id, autoSave: true);
        }

        public async Task GetContextInfo()
        {
            var currentTenant = CurrentTenant;
            var currentUser = CurrentUser.UserName;
            var guid = GuidFactory.NewGuid;
            var token = CurrentToken.RequestToken;

            await Task.CompletedTask;
        }

        public async Task<PostDto> GetDto()
        {
            var entities = await _repository.GetPagedListAsync(0, 1);
            var entity = entities.FirstOrDefault()
                ?? throw new InvalidOperationException("No TestEntity found. Call PostDto first or ensure the database is seeded.");

            return Mapper.Map<PostDto>(entity);
        }

        public async Task PostDto(PostDto dto)
        {
            var entity = new TestEntity
            {
                Id = GuidFactory.NewGuid,
                Name = dto.Name ?? string.Empty,
                Age = dto.Age
            };

            await _repository.InsertAsync(entity, autoSave: true);
        }

        public async Task PutDto(PostDto dto)
        {
            if (dto.Id is null || dto.Id == Guid.Empty)
                throw new ArgumentException("Id is required for update.", nameof(dto));

            var entity = await _repository.GetFirstAsync(e => e.Id == dto.Id.Value);
            entity.Name = dto.Name ?? string.Empty;
            entity.Age = dto.Age;

            await _repository.UpdateAsync(entity, autoSave: true);
        }
    }
}
