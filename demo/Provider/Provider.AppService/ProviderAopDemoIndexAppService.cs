using EasyCore.AspNetCore.Mvc.AppService;
using EasyCore.Dependency;

namespace Provider.AppService;

/// <summary>Index of all AOP placement demos (Dynamic API + Controller).</summary>
public interface IProviderAopDemoIndexAppService : ITransientDependency
{
    Task<object> GetIndexAsync();
}

public sealed class ProviderAopDemoIndexAppService : EasyCoreAppService, IProviderAopDemoIndexAppService
{
    public Task<object> GetIndexAsync()
    {
        object index = new
        {
            tip = "Placement demos for Invocation / Polly / Redis on Dynamic API + Controller + remote interface.",
            placements = new object[]
            {
                new
                {
                    id = "A",
                    where = "Interface type",
                    attrs = "[Audit] on IProviderAopIfaceTypeAppService",
                    provider = new[]
                    {
                        "GET /api/ProviderAopIfaceType/GetPingAsync",
                        "GET /api/ProviderAopIfaceType/GetEchoAsync?text=hi"
                    },
                    consumer = new[]
                    {
                        "GET /api/ConsumerTest/GetAopIfaceTypePingAsync"
                    },
                    note = "Provider Filter + Consumer Castle"
                },
                new
                {
                    id = "B",
                    where = "Interface method",
                    attrs = "[ServerCache]/[PollyConfig] on marked methods only",
                    provider = new[]
                    {
                        "GET /api/ProviderAopIfaceMethod/GetCachedAsync?key=demo",
                        "GET /api/ProviderAopIfaceMethod/GetUnstableAsync",
                        "GET /api/ProviderAopIfaceMethod/GetPlainAsync?key=x"
                    },
                    consumer = new[]
                    {
                        "GET /api/ConsumerTest/GetAopIfaceMethodCachedAsync?key=demo",
                        "GET /api/ConsumerTest/GetAopIfaceMethodUnstableAsync"
                    },
                    note = "GetPlainAsync has no AOP"
                },
                new
                {
                    id = "C",
                    where = "Interface combo (remote dynamic proxy)",
                    attrs = "[Audit] + [PollyConfig] circuit breaker / retry + [ServerCache]",
                    provider = new[]
                    {
                        "GET /api/ProviderAopCombo/GetUnstableAsync (circuit breaker: fail 3 → open 5s)",
                        "GET /api/ProviderAopCombo/GetCachedAsync?key=demo",
                        "GET /api/ProviderAopCombo/GetStackedAsync?key=x"
                    },
                    consumer = new[]
                    {
                        "GET /api/ConsumerTest/GetAopComboUnstableAsync",
                        "GET /api/ConsumerTest/GetAopComboCachedAsync?key=demo",
                        "GET /api/ConsumerTest/GetAopComboStackedAsync?key=x"
                    },
                    note = "Same contract attributes on both sides"
                },
                new
                {
                    id = "D+E",
                    where = "AppService class + method (Dynamic API)",
                    attrs = "class [Audit]; method [ServerCache]/[PollyConfig]",
                    provider = new[]
                    {
                        "GET /api/ProviderAopImplPlacement/GetFromClassAsync?name=x",
                        "GET /api/ProviderAopImplPlacement/GetFromMethodCachedAsync?key=demo",
                        "GET /api/ProviderAopImplPlacement/GetFromMethodUnstableAsync",
                        "GET /api/ProviderAopImplPlacement/GetPlainAsync?name=x"
                    },
                    consumer = Array.Empty<string>(),
                    note = "Provider Filter only — not remoted (class/method attrs invisible to DispatchProxy)"
                },
                new
                {
                    id = "F",
                    where = "MVC Controller / Action (API)",
                    attrs = "controller [Audit]; action [ServerCache]/[PollyConfig]",
                    provider = new[]
                    {
                        "GET /api/aop-controller/ping",
                        "GET /api/aop-controller/cached?key=demo",
                        "GET /api/aop-controller/unstable"
                    },
                    consumer = Array.Empty<string>(),
                    note = "Classic IFilterFactory path"
                }
            }
        };

        return Task.FromResult(index);
    }
}
