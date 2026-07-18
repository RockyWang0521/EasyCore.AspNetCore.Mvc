using System.Security.Claims;
using EasyCore.AspNetCore.Mvc.AppService;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace EasyCore.AspNetCore.Mvc.Tests;

public class CurrentUserTests
{
    [Fact]
    public void FindClaims_Filters_By_ClaimType()
    {
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "alice"),
                new Claim(ClaimTypes.Role, "admin"),
                new Claim(ClaimTypes.Role, "user"),
                new Claim("tenant", "t1")
            }, "test"))
        };

        var accessor = new HttpContextAccessor { HttpContext = context };
        var services = new ServiceCollection();
        services.AddSingleton<IHttpContextAccessor>(accessor);
        services.AddEasyCoreAppServices();

        using var provider = services.BuildServiceProvider();
        var currentUser = provider.GetRequiredService<ICurrentUser>();

        var roles = currentUser.FindClaims(ClaimTypes.Role);
        Assert.Equal(2, roles.Length);
        Assert.All(roles, c => Assert.Equal(ClaimTypes.Role, c.Type));
    }
}
