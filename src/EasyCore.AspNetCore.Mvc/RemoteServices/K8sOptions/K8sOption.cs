namespace EasyCore.AspNetCore.Mvc.RemoteServices.K8sOptions
{
    /// <summary>
    /// Options used when building Kubernetes DNS base addresses for remote clients.
    /// </summary>
    public class K8sOption
    {
# pragma warning disable CS8618

        /// <summary>
        /// Gets or sets the Kubernetes namespace (for example, <c>default</c>).
        /// </summary>
        public string K8sNamespace { set; get; }

        /// <summary>
        /// Gets or sets the cluster DNS suffix (for example, <c>svc.cluster.local</c>).
        /// </summary>
        public string K8sClusterDomain { set; get; }

# pragma warning restore CS8618
    }
}
