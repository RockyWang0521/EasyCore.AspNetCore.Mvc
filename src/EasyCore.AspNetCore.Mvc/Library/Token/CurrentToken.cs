namespace EasyCore.AspNetCore.Mvc.AppService
{
    /// <summary>
    /// Default <see cref="ICurrentToken"/> implementation that reads the <c>Authorization</c> header.
    /// </summary>
    internal class CurrentToken : ICurrentToken
    {
        /// <summary>
        /// Provides access to the current HTTP context.
        /// </summary>
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentToken"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        public CurrentToken(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

        /// <inheritdoc />
        public string RequestToken => GetRequestToken();

        /// <summary>
        /// Extracts the bearer token from the current request authorization header.
        /// </summary>
        /// <returns>The token value without the <c>Bearer </c> prefix.</returns>
        string GetRequestToken()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            var request = httpContext!.Request;

            var token = request.Headers["Authorization"].ToString();

            token = token.Replace("Bearer ", "");

            return token;
        }
    }
}
