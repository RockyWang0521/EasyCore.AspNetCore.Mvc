using System.Security.Claims;

namespace EasyCore.AspNetCore.Mvc.AppService
{
    /// <summary>
    /// Default <see cref="ICurrentUser"/> implementation that reads from <see cref="IHttpContextAccessor"/>.
    /// </summary>
    internal class CurrentUser : ICurrentUser
    {
        /// <summary>
        /// Provides access to the current HTTP context.
        /// </summary>
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentUser"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        public CurrentUser(IHttpContextAccessor httpContextAccessor)
            => _httpContextAccessor = httpContextAccessor;

        /// <summary>
        /// Gets the current user principal from the HTTP context, if any.
        /// </summary>
        private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        /// <inheritdoc />
        public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

        /// <inheritdoc />
        public string? UserName => User?.Identity?.Name;

        /// <inheritdoc />
        public ClaimsPrincipal PrincipalUser => User ?? new ClaimsPrincipal();

        /// <inheritdoc />
        public Claim[] FindClaims(string claimType)
        {
            if (string.IsNullOrEmpty(claimType))
                return Array.Empty<Claim>();

            var user = User;
            if (user == null)
                return Array.Empty<Claim>();

            return user.FindAll(claimType).ToArray();
        }
    }
}
