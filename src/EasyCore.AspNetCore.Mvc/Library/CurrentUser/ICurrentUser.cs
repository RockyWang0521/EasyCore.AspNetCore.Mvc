using System.Security.Claims;

namespace EasyCore.AspNetCore.Mvc.AppService
{
    /// <summary>
    /// Provides access to the current authenticated user from the HTTP context.
    /// </summary>
    public interface ICurrentUser
    {
        /// <summary>
        /// Gets the <see cref="ClaimsPrincipal"/> for the current user.
        /// </summary>
        ClaimsPrincipal PrincipalUser { get; }

        /// <summary>
        /// Gets a value indicating whether the current user is authenticated.
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// Gets the user name from the current identity, if available.
        /// </summary>
        string? UserName { get; }

        /// <summary>
        /// Finds all claims of the specified type for the current user.
        /// </summary>
        /// <param name="claimType">The claim type to search for.</param>
        /// <returns>An array of matching claims, or an empty array when none are found.</returns>
        Claim[] FindClaims(string claimType);
    }
}
