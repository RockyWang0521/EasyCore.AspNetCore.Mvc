namespace EasyCore.AspNetCore.Mvc.RemoteServices.K8sOptions
{
    public class K8sOption
    {
# pragma warning disable CS8618 

        /// <summary>
        /// The name of the Kubernetes cluster.
        /// </summary>
        public string K8sNamespace { set; get; }

        /// <summary>
        /// The domain of the Kubernetes cluster.
        /// </summary>
        public string K8sClusterDomain { set; get; }

# pragma warning restore CS8618 
    }
}
