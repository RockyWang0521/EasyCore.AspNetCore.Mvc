namespace EasyCore.AspNetCore.Mvc.RemoteServices
{
    /// <summary>
    /// Provides HTTP headers that should be forwarded on remote API calls.
    /// </summary>
    public interface IRemoteRequestHeaderProvider
    {
        /// <summary>
        /// Gets the headers to attach to an outbound remote request.
        /// </summary>
        /// <returns>A dictionary of header names and values.</returns>
        IDictionary<string, string> GetHeaders();
    }
}
