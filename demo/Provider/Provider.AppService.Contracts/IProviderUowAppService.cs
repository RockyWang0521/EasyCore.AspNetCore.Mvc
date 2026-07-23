using EasyCore.Dependency;
using EasyCore.UnitOfWork;
using Provider.EFCore;

namespace Provider.AppService.Contracts;

/// <summary>
/// Placement U1 — [SaveChanges] on interface type (Provider Dynamic API Filter path).
/// Not remoted (no ApiHost) — persistence belongs on the Provider.
/// </summary>
[SaveChanges(typeof(TestDbContext))]
public interface IProviderUowIfaceTypeAppService : ITransientDependency
{
    Task<object> PostInsertAsync(string name);
}

/// <summary>
/// Placement U2 — [SaveChanges] on interface method only.
/// </summary>
public interface IProviderUowIfaceMethodAppService : ITransientDependency
{
    [SaveChanges(true, typeof(TestDbContext))]
    Task<object> PostInsertTransactionalAsync(string name);

    Task<object> GetPlainAsync(string name);
}
