using Consul;

namespace EasyCore.AspNetCore.Mvc.RemoteServices
{
    /// <summary>
    /// Consul service discovery implementation.
    /// </summary>
    public class ConsulServiceDiscovery
    {
        private readonly IConsulClient _consulClient;
        private readonly ILogger<ConsulServiceDiscovery> _logger;

        public ConsulServiceDiscovery(
            IConsulClient consulClient, 
            ILogger<ConsulServiceDiscovery> logger)
        {
            _consulClient = consulClient;

            _logger = logger;
        }

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
