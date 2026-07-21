using EasyCore.Invocation;

namespace Provider.AppService.Contracts.Invocations;

public sealed class AuditInvocation : IInvocation
{
    public async ValueTask<object?> InvokeAsync(InvocationContext context, InvocationDelegate next)
    {
        Console.WriteLine($"[Audit] Before {context.TargetType.Name}.{context.MethodName}");
        try
        {
            var result = await next().ConfigureAwait(false);
            Console.WriteLine($"[Audit] After  {context.TargetType.Name}.{context.MethodName}");
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Audit] Error  {context.MethodName}: {ex.Message}");
            throw;
        }
        finally
        {
            Console.WriteLine($"[Audit] Finally {context.MethodName}");
        }
    }
}
