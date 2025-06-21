using System.Security.Claims;

namespace EasyCore.AspNetCore.Mvc.AppService
{
    internal class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUser(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

        public bool IsAuthenticated => _httpContextAccessor!.HttpContext!.User.Identity!.IsAuthenticated;

        public string? UserName => _httpContextAccessor!.HttpContext!.User.Identity!.Name;

        public ClaimsPrincipal PrincipalUser => _httpContextAccessor!.HttpContext!.User;

        public Claim[] FindClaims(string claimType)
        {
            return _httpContextAccessor!.HttpContext!.User.Claims.ToArray();
        }
    }
}
