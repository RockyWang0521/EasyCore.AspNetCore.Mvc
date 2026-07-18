using Consul;

namespace EasyCore.AspNetCore.Mvc.RemoteServices
{
    /// <summary>
    /// Discovers healthy service instances from Consul and returns a base URI.
    /// </summary>
    public class ConsulServiceDiscovery
    {
        /// <summary>
        /// The Consul client used for health lookups.
        /// </summary>
        private readonly IConsulClient _consulClient;

        /// <summary>
        /// Logger for discovery warnings and failures.
        /// </summary>
        private readonly ILogger<ConsulServiceDiscovery> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsulServiceDiscovery"/> class.
        /// </summary>
        /// <param name="consulClient">The Consul client.</param>
        /// <param name="logger">The logger instance.</param>
        public ConsulServiceDiscovery(
            IConsulClient consulClient,
            ILogger<ConsulServiceDiscovery> logger)
        {
            _consulClient = consulClient;

            _logger = logger;
        }

        /// <summary>
        /// Resolves a random healthy instance URI for the specified Consul service name.
        /// </summary>
        /// <param name="serviceName">The Consul service name.</param>
        /// <returns>The instance base URI, or <c>null</c> when no healthy instance is found.</returns>
        public async Task<Uri?> GetServiceUriAsync(string serviceName)
        {
            try
            {
                var services = await _consulClient.Health.Service(serviceName, "", true);

                var healthyServices = services.Response;

                if (healthyServices == null || healthyServices.Length == 0)
                {
                    _logger.LogWarning("No healthy instance found for {ServiceName}", serviceName);

                    return null;
                }

                var random = new Random();

                var serviceEntry = healthyServices[random.Next(healthyServices.Length)];

                if (serviceEntry == null)
                {
                    _logger.LogWarning("Consul service not found: {ServiceName}", serviceName);

                    return null;
                }

                var address = serviceEntry.Service.Address;

                var port = serviceEntry.Service.Port;

                var uri = new Uri($"http://{address}:{port}");

                return uri;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Consul service discovery error for {ServiceName}", serviceName);

                return null;
            }
        }
    }
}
