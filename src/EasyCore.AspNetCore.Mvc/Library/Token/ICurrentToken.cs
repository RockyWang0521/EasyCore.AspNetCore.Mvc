namespace EasyCore.AspNetCore.Mvc.AppService
{
    public interface ICurrentToken
    {
        /// <summary>
        /// Gets the current token.
        /// </summary>
        string? RequestToken { get; }
    }
}
