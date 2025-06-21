namespace EasyCore.AspNetCore.Mvc.RemoteServices
{
    /// <summary>
    /// Provides the headers for a remote request.
    /// </summary>
    public interface IRemoteRequestHeaderProvider
    {
        IDictionary<string, string> GetHeaders();
    }
}