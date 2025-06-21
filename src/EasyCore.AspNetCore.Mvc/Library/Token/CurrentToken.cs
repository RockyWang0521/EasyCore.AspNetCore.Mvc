namespace EasyCore.AspNetCore.Mvc.AppService
{
    internal class CurrentToken : ICurrentToken
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentToken(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

        public string RequestToken => GetRequestToken();

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
