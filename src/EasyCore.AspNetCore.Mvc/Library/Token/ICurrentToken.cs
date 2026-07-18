namespace EasyCore.AspNetCore.Mvc.AppService
{
    /// <summary>
    /// Provides access to the bearer token on the current HTTP request.
    /// </summary>
    public interface ICurrentToken
    {
        /// <summary>
        /// Gets the authorization token from the current request without the <c>Bearer </c> prefix.
        /// </summary>
        string? RequestToken { get; }
    }
}
