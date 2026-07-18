using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using EasyCore.AspNetCore.Mvc.RemoteServices.NacosOptions;
using Microsoft.Extensions.Options;

namespace EasyCore.AspNetCore.Mvc.RemoteServices
{
    /// <summary>
    /// Discovers healthy service instances from Nacos Naming OpenAPI and returns a base URI.
    /// </summary>
    public class NacosServiceDiscovery
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IOptionsMonitor<NacosOption> _options;
        private readonly ILogger<NacosServiceDiscovery> _logger;
        private readonly SemaphoreSlim _tokenLock = new(1, 1);
        private string? _accessToken;
        private DateTimeOffset _accessTokenExpiresAt = DateTimeOffset.MinValue;

        /// <summary>
        /// Named <see cref="HttpClient"/> used for Nacos OpenAPI calls.
        /// </summary>
        public const string HttpClientName = "EasyCore.Nacos.OpenApi";

        /// <summary>
        /// Initializes a new instance of the <see cref="NacosServiceDiscovery"/> class.
        /// </summary>
        public NacosServiceDiscovery(
            IHttpClientFactory httpClientFactory,
            IOptionsMonitor<NacosOption> options,
            ILogger<NacosServiceDiscovery> logger)
        {
            _httpClientFactory = httpClientFactory;
            _options = options;
            _logger = logger;
        }

        /// <summary>
        /// Resolves a random healthy instance URI for the specified Nacos service.
        /// </summary>
        public async Task<Uri?> GetServiceUriAsync(
            string serviceName,
            string? group = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
                throw new ArgumentException("Nacos service name is required.", nameof(serviceName));

            var option = _options.CurrentValue;
            var servers = ParseServerAddresses(option.ServerAddresses);
            if (servers.Count == 0)
            {
                _logger.LogWarning("Nacos ServerAddresses is not configured.");
                return null;
            }

            var groupName = string.IsNullOrWhiteSpace(group) ? option.GroupName : group;
            if (string.IsNullOrWhiteSpace(groupName))
                groupName = "DEFAULT_GROUP";

            Exception? lastError = null;
            foreach (var server in servers)
            {
                try
                {
                    var uri = await TryDiscoverAsync(server, option, serviceName, groupName, cancellationToken)
                        .ConfigureAwait(false);
                    if (uri != null)
                        return uri;
                }
                catch (Exception ex)
                {
                    lastError = ex;
                    _logger.LogWarning(ex, "Nacos discovery failed against {Server}", server);
                }
            }

            if (lastError != null)
                _logger.LogError(lastError, "Nacos service discovery error for {ServiceName}", serviceName);

            return null;
        }

        private async Task<Uri?> TryDiscoverAsync(
            string server,
            NacosOption option,
            string serviceName,
            string groupName,
            CancellationToken cancellationToken)
        {
            var client = _httpClientFactory.CreateClient(HttpClientName);
            var baseServer = server.TrimEnd('/');
            var accessToken = await GetAccessTokenAsync(client, baseServer, option, cancellationToken)
                .ConfigureAwait(false);

            var query =
                $"serviceName={Uri.EscapeDataString(serviceName)}" +
                $"&groupName={Uri.EscapeDataString(groupName)}" +
                "&healthyOnly=true";

            if (!string.IsNullOrWhiteSpace(option.Namespace))
                query += $"&namespaceId={Uri.EscapeDataString(option.Namespace)}";

            if (!string.IsNullOrWhiteSpace(accessToken))
                query += $"&accessToken={Uri.EscapeDataString(accessToken)}";
            else
            {
                // Compatibility with older Nacos OpenAPI auth via query credentials.
                if (!string.IsNullOrWhiteSpace(option.UserName))
                    query += $"&username={Uri.EscapeDataString(option.UserName)}";
                if (!string.IsNullOrWhiteSpace(option.Password))
                    query += $"&password={Uri.EscapeDataString(option.Password)}";
            }

            var url = $"{baseServer}/nacos/v1/ns/instance/list?{query}";
            using var response = await client.GetAsync(url, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                throw new InvalidOperationException(
                    $"Nacos instance list failed ({(int)response.StatusCode}): {body}");
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            var payload = await JsonSerializer.DeserializeAsync<NacosInstanceListResponse>(
                stream,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                cancellationToken).ConfigureAwait(false);

            var hosts = payload?.Hosts?
                .Where(h => h != null && h.Healthy && h.Enabled && !string.IsNullOrWhiteSpace(h.Ip) && h.Port > 0)
                .ToList();

            if (hosts == null || hosts.Count == 0)
            {
                _logger.LogWarning(
                    "No healthy Nacos instance found for {ServiceName} (group={Group}) on {Server}",
                    serviceName,
                    groupName,
                    server);
                return null;
            }

            var host = hosts[Random.Shared.Next(hosts.Count)]!;
            return BuildInstanceUri(host.Ip!, host.Port);
        }

        private async Task<string?> GetAccessTokenAsync(
            HttpClient client,
            string baseServer,
            NacosOption option,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(option.UserName) || string.IsNullOrWhiteSpace(option.Password))
                return null;

            if (!string.IsNullOrEmpty(_accessToken) && DateTimeOffset.UtcNow < _accessTokenExpiresAt)
                return _accessToken;

            await _tokenLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (!string.IsNullOrEmpty(_accessToken) && DateTimeOffset.UtcNow < _accessTokenExpiresAt)
                    return _accessToken;

                using var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["username"] = option.UserName!,
                    ["password"] = option.Password!
                });

                using var response = await client
                    .PostAsync($"{baseServer}/nacos/v1/auth/login", content, cancellationToken)
                    .ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                    throw new InvalidOperationException(
                        $"Nacos login failed ({(int)response.StatusCode}): {body}");
                }

                var login = await response.Content
                    .ReadFromJsonAsync<NacosLoginResponse>(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                if (string.IsNullOrWhiteSpace(login?.AccessToken))
                    throw new InvalidOperationException("Nacos login response did not include accessToken.");

                var ttlSeconds = login.TokenTtl > 0 ? login.TokenTtl : 18000;
                _accessToken = login.AccessToken;
                _accessTokenExpiresAt = DateTimeOffset.UtcNow.AddSeconds(Math.Max(60, ttlSeconds - 60));
                return _accessToken;
            }
            finally
            {
                _tokenLock.Release();
            }
        }

        /// <summary>
        /// Picks the first non-empty server address from a comma-separated list.
        /// </summary>
        internal static string? PickServerAddress(string? serverAddresses)
            => ParseServerAddresses(serverAddresses).FirstOrDefault();

        /// <summary>
        /// Parses comma-separated Nacos server addresses.
        /// </summary>
        internal static IReadOnlyList<string> ParseServerAddresses(string? serverAddresses)
        {
            if (string.IsNullOrWhiteSpace(serverAddresses))
                return Array.Empty<string>();

            return serverAddresses
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToArray();
        }

        /// <summary>
        /// Builds an HTTP base URI for an instance, wrapping IPv6 addresses in brackets.
        /// </summary>
        internal static Uri BuildInstanceUri(string ip, int port)
        {
            var host = ip.Contains(':') && !ip.StartsWith('[') ? $"[{ip}]" : ip;
            return new Uri($"http://{host}:{port}/");
        }

        /// <summary>
        /// Parses a Nacos instance-list JSON payload into healthy host URIs (for unit tests).
        /// </summary>
        internal static IReadOnlyList<Uri> ParseHealthyInstanceUris(string json)
        {
            var payload = JsonSerializer.Deserialize<NacosInstanceListResponse>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return payload?.Hosts?
                .Where(h => h != null && h.Healthy && h.Enabled && !string.IsNullOrWhiteSpace(h.Ip) && h.Port > 0)
                .Select(h => BuildInstanceUri(h!.Ip!, h.Port))
                .ToList()
                ?? (IReadOnlyList<Uri>)Array.Empty<Uri>();
        }

        private sealed class NacosLoginResponse
        {
            [JsonPropertyName("accessToken")]
            public string? AccessToken { get; set; }

            [JsonPropertyName("tokenTtl")]
            public long TokenTtl { get; set; }
        }

        private sealed class NacosInstanceListResponse
        {
            [JsonPropertyName("hosts")]
            public List<NacosHost?>? Hosts { get; set; }
        }

        private sealed class NacosHost
        {
            [JsonPropertyName("ip")]
            public string? Ip { get; set; }

            [JsonPropertyName("port")]
            public int Port { get; set; }

            [JsonPropertyName("healthy")]
            public bool Healthy { get; set; } = true;

            [JsonPropertyName("enabled")]
            public bool Enabled { get; set; } = true;
        }
    }
}
