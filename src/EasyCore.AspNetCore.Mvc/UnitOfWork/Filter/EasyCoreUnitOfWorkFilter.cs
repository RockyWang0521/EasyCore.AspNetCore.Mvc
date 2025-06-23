using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace EasyCore.AspNetCore.Mvc
{
    /// <summary>
    /// A filter that ensures that all changes to the <see cref="DbContext"/> instances are saved when the action is executed.
    /// </summary>
    public class EasyCoreUnitOfWorkFilter : IAsyncActionFilter
    {
        private readonly IEnumerable<DbContext> _dbContexts;

        public EasyCoreUnitOfWorkFilter(IEnumerable<DbContext> dbContexts)
        {
            _dbContexts = dbContexts;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.Controller is not IEasyCoreUnitOfWork)
            {
                await next();

                return;
            }

            var executedContext = await next();

            if (executedContext.Exception == null || executedContext.ExceptionHandled)
            {
                foreach (var dbContext in _dbContexts)
                {
                    if (dbContext.ChangeTracker.HasChanges())
                    {
                        await dbContext.SaveChangesAsync();
                    }
                }
            }
        }
    }
}
