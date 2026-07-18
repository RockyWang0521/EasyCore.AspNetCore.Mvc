using Consul;

namespace EasyCore.AspNetCore.Mvc.RemoteServices
{
    /// <summary>
    /// Discovers healthy service instances from Consul and returns a base URI.
    /// </summary>
    public class ConsulServiceDiscovery
    {
        private readonly IConsulClient _consulClient;
        private readonly ILogger<ConsulServiceDiscovery> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsulServiceDiscovery"/> class.
        /// </summary>
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
        public async Task<Uri?> GetServiceUriAsync(string serviceName)
        {
            try
            {
                var services = await _consulClient.Health.Service(serviceName, "", true).ConfigureAwait(false);
                var healthyServices = services.Response;

                if (healthyServices == null || healthyServices.Length == 0)
                {
                    _logger.LogWarning("No healthy instance found for {ServiceName}", serviceName);
                    return null;
                }

                var serviceEntry = healthyServices[Random.Shared.Next(healthyServices.Length)];
                if (serviceEntry == null)
                {
                    _logger.LogWarning("Consul service not found: {ServiceName}", serviceName);
                    return null;
                }

                var address = serviceEntry.Service.Address;
                if (string.IsNullOrWhiteSpace(address))
                    address = serviceEntry.Node?.Address;

                if (string.IsNullOrWhiteSpace(address))
                {
                    _logger.LogWarning(
                        "Consul service {ServiceName} has empty Service.Address and Node.Address",
                        serviceName);
                    return null;
                }

                var port = serviceEntry.Service.Port;
                return BuildInstanceUri(address, port);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Consul service discovery error for {ServiceName}", serviceName);
                return null;
            }
        }

        /// <summary>
        /// Builds an HTTP base URI for an instance, wrapping IPv6 addresses in brackets.
        /// </summary>
        internal static Uri BuildInstanceUri(string address, int port)
        {
            var host = address.Contains(':') && !address.StartsWith('[')
                ? $"[{address}]"
                : address;
            return new Uri($"http://{host}:{port}/");
        }
    }
}
