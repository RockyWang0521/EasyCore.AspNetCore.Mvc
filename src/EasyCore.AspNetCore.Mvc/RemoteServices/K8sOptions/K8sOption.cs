namespace EasyCore.AspNetCore.Mvc.RemoteServices.K8sOptions
{
    /// <summary>
    /// Options used when building Kubernetes DNS base addresses for remote clients.
    /// </summary>
    public class K8sOption
    {
        /// <summary>
        /// Gets or sets the Kubernetes namespace (for example, <c>default</c>).
        /// </summary>
        public string K8sNamespace { get; set; } = "default";

        /// <summary>
        /// Gets or sets the cluster domain (for example, <c>cluster.local</c>).
        /// The final host is <c>{service}.{namespace}.svc.{K8sClusterDomain}</c>.
        /// </summary>
        public string K8sClusterDomain { get; set; } = "cluster.local";
    }
}
