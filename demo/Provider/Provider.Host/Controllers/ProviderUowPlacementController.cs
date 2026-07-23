using EasyCore.UnitOfWork;
using Microsoft.AspNetCore.Mvc;
using Provider.EFCore;
using Provider.EFCore.Entity;
using Provider.EFCore.Repository;

namespace Provider.Host.Controllers;

/// <summary>
/// Placement U4 — [SaveChanges] on MVC Controller / Action (API IFilterFactory path).
/// </summary>
[ApiController]
[Route("api/uow-controller")]
public sealed class ProviderUowPlacementController : ControllerBase
{
    private readonly ITestEntityRepository _repository;

    public ProviderUowPlacementController(ITestEntityRepository repository) => _repository = repository;

    [HttpPost("insert")]
    [SaveChanges(typeof(TestDbContext))]
    public async Task<object> PostInsert([FromQuery] string name = "controller")
    {
        var entity = await _repository.InsertAsync(new TestEntity
        {
            Id = Guid.NewGuid(),
            Name = name,
            Age = 5,
            CreateTime = DateTime.Now,
            ConcurrencyStamp = Guid.NewGuid().ToString("N")
        }).ConfigureAwait(false);

        Console.WriteLine($"  [UowController] Inserted Id={entity.Id}");
        return new { entity.Id, entity.Name, path = "controller-action" };
    }
}
