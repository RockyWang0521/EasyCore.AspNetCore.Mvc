using System.Security.Claims;

namespace EasyCore.AspNetCore.Mvc.AppService
{
    public interface ICurrentUser
    {
        /// <summary>
        /// this is the interface of current user
        /// </summary>
        ClaimsPrincipal PrincipalUser { get; }

        /// <summary>
        /// Indicates whether the current user is authenticated.
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// Gets the user identifier.
        /// </summary>
        string? UserName { get; }

        /// <summary>
        /// Gets the user Claims
        /// </summary>
        /// <param name="claimType"></param>
        /// <returns></returns>
        Claim[] FindClaims(string claimType);
    }
}
