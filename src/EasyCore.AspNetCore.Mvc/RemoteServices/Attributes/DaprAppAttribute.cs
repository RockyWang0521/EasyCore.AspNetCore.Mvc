namespace EasyCore.AspNetCore.Mvc.RemoteServices
{
    /// <summary>
    /// Marks a remote contract to invoke the target app through the local Dapr HTTP sidecar.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
    public sealed class DaprAppAttribute : Attribute
    {
        /// <summary>
        /// Gets the Dapr application id used in service invocation.
        /// </summary>
        public string AppId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DaprAppAttribute"/> class.
        /// </summary>
        /// <param name="appId">The Dapr app id of the remote service.</param>
        public DaprAppAttribute(string appId)
        {
            AppId = appId ?? throw new ArgumentNullException(nameof(appId));
        }
    }
}
