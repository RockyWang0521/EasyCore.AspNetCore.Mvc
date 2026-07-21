using EasyCore.Invocation;
using Provider.AppService.Contracts.Invocations;

namespace Provider.AppService.Contracts.Attributes;

/// <summary>Demo Invocation wrapper — logs before/after AppService or remote proxy calls.</summary>
public sealed class AuditAttribute : InvocationAttribute<AuditInvocation>
{
    public AuditAttribute()
    {
        Order = 0;
    }
}
