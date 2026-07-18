using System.Collections;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace EasyCore.AspNetCore.Mvc.RemoteServices
{
    /// <summary>
    /// Shared <see cref="DispatchProxy"/> that forwards interface method calls over HTTP.
    /// </summary>
    public class RemoteHttpClientProxy : DispatchProxy
    {
        /// <summary>
        /// The HTTP client used to send remote requests.
        /// </summary>
        private HttpClient? _httpClient;

        /// <summary>
        /// The route prefix prepended to each action name (for example, <c>api/ProviderTest</c>).
        /// </summary>
        private string _routePrefix = "";

        /// <summary>
        /// Optional provider that supplies headers to forward on each request.
        /// </summary>
        private IRemoteRequestHeaderProvider? _headerProvider;

        /// <summary>
        /// Configures the proxy with an HTTP client, route prefix, and optional header provider.
        /// </summary>
        /// <param name="httpClient">The HTTP client instance.</param>
        /// <param name="routePrefix">The API route prefix.</param>
        /// <param name="headerProvider">Optional header provider.</param>
        public void Configure(HttpClient httpClient, string routePrefix, IRemoteRequestHeaderProvider? headerProvider)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _routePrefix = routePrefix?.TrimEnd('/') ?? "";
            _headerProvider = headerProvider;
        }

        /// <summary>
        /// Builds a default route prefix from an interface type name
        /// (strips leading <c>I</c>, <c>AppService</c>, and <c>Service</c>).
        /// </summary>
        /// <param name="interfaceType">The remote contract interface type.</param>
        /// <returns>A route prefix such as <c>api/ProviderTest</c>.</returns>
        public static string BuildRoutePrefix(Type interfaceType)
        {
            if (interfaceType == null) throw new ArgumentNullException(nameof(interfaceType));

            var name = interfaceType.Name;
            if (name.StartsWith('I') && name.Length > 1)
                name = name[1..];

            name = name.Replace("AppService", string.Empty, StringComparison.Ordinal)
                .Replace("Service", string.Empty, StringComparison.Ordinal);

            return $"api/{name}";
        }

        /// <summary>
        /// Infers the HTTP verb from a method name prefix.
        /// </summary>
        /// <param name="name">The method name.</param>
        /// <returns>One of <c>GET</c>, <c>POST</c>, <c>PUT</c>, or <c>DELETE</c>.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the verb cannot be inferred.</exception>
        public static string GetHttpMethodFromName(string name)
        {
            if (name.StartsWith("Get", StringComparison.OrdinalIgnoreCase))
                return "GET";
            if (name.StartsWith("Post", StringComparison.OrdinalIgnoreCase))
                return "POST";
            if (name.StartsWith("Put", StringComparison.OrdinalIgnoreCase))
                return "PUT";
            if (name.StartsWith("Delete", StringComparison.OrdinalIgnoreCase))
                return "DELETE";

            throw new InvalidOperationException(
                $"Cannot infer HTTP verb from method name '{name}'. Use Get*/Post*/Put*/Delete* naming.");
        }

        /// <summary>
        /// Builds a relative route with a query string using real parameter names.
        /// </summary>
        /// <param name="route">The base relative route.</param>
        /// <param name="method">The invoked interface method.</param>
        /// <param name="args">The method arguments.</param>
        /// <returns>The route including query string when applicable.</returns>
        public static string AppendQuery(string route, MethodInfo method, object?[]? args)
        {
            if (args == null || args.Length == 0)
                return route;

            if (args.Length == 1 && args[0] is IDictionary dict)
            {
                var query = string.Join("&", dict.Keys.Cast<object>().Where(k => k != null).Select(k =>
                    $"{Uri.EscapeDataString(k.ToString()!)}={Uri.EscapeDataString(dict[k]?.ToString() ?? string.Empty)}"));
                return string.IsNullOrEmpty(query) ? route : $"{route}?{query}";
            }

            var parameters = method.GetParameters();
            var parts = new List<string>();

            for (var i = 0; i < args.Length && i < parameters.Length; i++)
            {
                if (args[i] == null)
                    continue;

                var name = parameters[i].Name ?? $"arg{i}";
                parts.Add($"{Uri.EscapeDataString(name)}={Uri.EscapeDataString(args[i]!.ToString()!)}");
            }

            return parts.Count == 0 ? route : $"{route}?{string.Join("&", parts)}";
        }

        /// <summary>
        /// Invokes the remote HTTP endpoint that corresponds to the intercepted interface method.
        /// </summary>
        /// <param name="targetMethod">The intercepted method.</param>
        /// <param name="args">The method arguments.</param>
        /// <returns>The deserialized result, a <see cref="Task"/>, or <c>null</c> for void methods.</returns>
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod == null)
                throw new ArgumentNullException(nameof(targetMethod));

            if (_httpClient == null)
                throw new InvalidOperationException("HttpClient is not configured.");

            var returnType = targetMethod.ReturnType;
            var httpMethod = GetHttpMethodFromName(targetMethod.Name);
            var route = $"{_routePrefix}/{targetMethod.Name}";

            if (returnType == typeof(Task))
                return InvokeVoidAsync(httpMethod, route, targetMethod, args);

            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                var resultType = returnType.GetGenericArguments()[0];
                var method = typeof(RemoteHttpClientProxy)
                    .GetMethod(nameof(InvokeWithResultAsyncGeneric), BindingFlags.Instance | BindingFlags.NonPublic)!
                    .MakeGenericMethod(resultType);

                return method.Invoke(this, new object[] { httpMethod, route, targetMethod, args! })!;
            }

            var syncResponse = SendRequest(httpMethod, route, targetMethod, args).GetAwaiter().GetResult();
            syncResponse.EnsureSuccessStatusCode();

            if (returnType == typeof(void))
                return null;

            var json = syncResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            return JsonSerializer.Deserialize(json, returnType, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        /// <summary>
        /// Adds forwarded headers from <see cref="_headerProvider"/> to the request.
        /// </summary>
        /// <param name="request">The outbound HTTP request.</param>
        private void AddHeaders(HttpRequestMessage request)
        {
            if (_headerProvider == null)
                return;

            foreach (var (key, value) in _headerProvider.GetHeaders())
                request.Headers.TryAddWithoutValidation(key, value);
        }

        /// <summary>
        /// Builds and sends an HTTP request for the given verb and route.
        /// </summary>
        /// <param name="httpMethod">The HTTP verb.</param>
        /// <param name="route">The relative route.</param>
        /// <param name="method">The interface method metadata.</param>
        /// <param name="args">The method arguments.</param>
        /// <returns>The HTTP response task.</returns>
        private Task<HttpResponseMessage> SendRequest(string httpMethod, string route, MethodInfo method, object?[]? args)
        {
            var uri = httpMethod is "GET" or "DELETE"
                ? new Uri(AppendQuery(route, method, args), UriKind.Relative)
                : new Uri(route, UriKind.Relative);

            var request = new HttpRequestMessage
            {
                Method = new HttpMethod(httpMethod),
                RequestUri = uri,
                Content = httpMethod is "GET" or "DELETE" ? null : CreateJsonContent(args)
            };

            AddHeaders(request);
            return _httpClient!.SendAsync(request);
        }

        /// <summary>
        /// Sends a remote request for methods that return a non-generic <see cref="Task"/>.
        /// </summary>
        private async Task InvokeVoidAsync(string httpMethod, string route, MethodInfo method, object?[]? args)
        {
            var response = await SendRequest(httpMethod, route, method, args).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Sends a remote request and deserializes the response body into <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The expected response type.</typeparam>
        private async Task<T> InvokeWithResultAsyncGeneric<T>(string httpMethod, string route, MethodInfo method, object?[]? args)
        {
            var response = await SendRequest(httpMethod, route, method, args).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        /// <summary>
        /// Serializes the first method argument as camelCase JSON content.
        /// </summary>
        /// <param name="args">The method arguments.</param>
        /// <returns>JSON HTTP content.</returns>
        private static HttpContent CreateJsonContent(object?[]? args)
        {
            var obj = args is { Length: > 0 } ? args[0] : null;
            var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            return new StringContent(json, Encoding.UTF8, "application/json");
        }
    }
}
