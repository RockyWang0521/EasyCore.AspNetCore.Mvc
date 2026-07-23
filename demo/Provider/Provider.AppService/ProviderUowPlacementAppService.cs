using EasyCore.AspNetCore.Mvc.AppService;
using EasyCore.UnitOfWork;
using Provider.AppService.Contracts;
using Provider.EFCore;
using Provider.EFCore.Entity;
using Provider.EFCore.Repository;

namespace Provider.AppService;

/// <summary>Placement U1 — SaveChanges on interface type.</summary>
public sealed class ProviderUowIfaceTypeAppService : EasyCoreAppService, IProviderUowIfaceTypeAppService
{
    private readonly ITestEntityRepository _repository;

    public ProviderUowIfaceTypeAppService(ITestEntityRepository repository) => _repository = repository;

    public async Task<object> PostInsertAsync(string name)
    {
        var entity = await _repository.InsertAsync(new TestEntity
        {
            Id = Guid.NewGuid(),
            Name = name,
            Age = 1,
            CreateTime = DateTime.Now,
            ConcurrencyStamp = Guid.NewGuid().ToString("N")
        }).ConfigureAwait(false);

        Console.WriteLine($"  [UowIfaceType] Inserted Id={entity.Id} (SaveChanges via interface type attr)");
        return new { entity.Id, entity.Name, path = "interface-type" };
    }
}

/// <summary>Placement U2 — SaveChanges on interface method.</summary>
public sealed class ProviderUowIfaceMethodAppService : EasyCoreAppService, IProviderUowIfaceMethodAppService
{
    private readonly ITestEntityRepository _repository;

    public ProviderUowIfaceMethodAppService(ITestEntityRepository repository) => _repository = repository;

    public async Task<object> PostInsertTransactionalAsync(string name)
    {
        var entity = await _repository.InsertAsync(new TestEntity
        {
            Id = Guid.NewGuid(),
            Name = name,
            Age = 2,
            CreateTime = DateTime.Now,
            ConcurrencyStamp = Guid.NewGuid().ToString("N")
        }).ConfigureAwait(false);

        Console.WriteLine($"  [UowIfaceMethod] Inserted Id={entity.Id} (transactional SaveChanges)");
        return new { entity.Id, entity.Name, path = "interface-method" };
    }

    public Task<object> GetPlainAsync(string name)
        => Task.FromResult<object>(new { name, path = "plain-no-savechanges" });
}

/// <summary>
/// Placement U3 — SaveChanges on AppService class / method (Dynamic API Filter).
/// </summary>
[SaveChanges(typeof(TestDbContext))]
public sealed class ProviderUowImplPlacementAppService : EasyCoreAppService
{
    private readonly ITestEntityRepository _repository;

    public ProviderUowImplPlacementAppService(ITestEntityRepository repository) => _repository = repository;

    public async Task<object> PostFromClassAsync(string name)
    {
        var entity = await _repository.InsertAsync(new TestEntity
        {
            Id = Guid.NewGuid(),
            Name = name,
            Age = 3,
            CreateTime = DateTime.Now,
            ConcurrencyStamp = Guid.NewGuid().ToString("N")
        }).ConfigureAwait(false);

        Console.WriteLine($"  [UowImpl] class-level SaveChanges Id={entity.Id}");
        return new { entity.Id, entity.Name, path = "appservice-class" };
    }

    [SaveChanges(true, typeof(TestDbContext))]
    public async Task<object> PostFromMethodTransactionalAsync(string name)
    {
        var entity = await _repository.InsertAsync(new TestEntity
        {
            Id = Guid.NewGuid(),
            Name = name,
            Age = 4,
            CreateTime = DateTime.Now,
            ConcurrencyStamp = Guid.NewGuid().ToString("N")
        }).ConfigureAwait(false);

        Console.WriteLine($"  [UowImpl] method-level transactional SaveChanges Id={entity.Id}");
        return new { entity.Id, entity.Name, path = "appservice-method" };
    }
}
