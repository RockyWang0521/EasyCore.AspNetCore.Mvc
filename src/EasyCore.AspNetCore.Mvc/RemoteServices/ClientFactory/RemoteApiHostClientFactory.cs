using System.Reflection;
using System.Text;
using System.Text.Json;

namespace EasyCore.AspNetCore.Mvc.RemoteServices
{
    /// <summary>
    /// A factory for creating remote API clients.
    /// </summary>
    public static class RemoteApiHostClientFactory
    {
        /// <summary>
        /// Create a remote API client.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="httpClient"></param>
        /// <param name="routePrefix"></param>
        /// <param name="headerProvider"></param>
        /// <returns></returns>
        public static T Create<T>(HttpClient httpClient, string? routePrefix = null, IRemoteRequestHeaderProvider? headerProvider = null) where T : class
        {
            if (string.IsNullOrWhiteSpace(routePrefix))
            {
                var interfaceName = typeof(T).Name;

                routePrefix = interfaceName.StartsWith("I") ? interfaceName.Substring(1) : interfaceName;

                routePrefix = routePrefix.Replace("AppService", "").Replace("Service", "");

                routePrefix = $"api/{routePrefix}";
            }

            var proxy = DispatchProxy.Create<T, HttpClientProxy>();

            (proxy as HttpClientProxy)!.Configure(httpClient, routePrefix, headerProvider);

            return proxy;
        }

        /// <summary>
        /// Create a remote API client.
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="routePrefix"></param>
        /// <param name="headerProvider"></param>
        /// <param name="iface"></param>
        /// <returns></returns>
        public static object Create(HttpClient httpClient, string? routePrefix, IRemoteRequestHeaderProvider? headerProvider, Type iface)
        {
            var method = typeof(RemoteApiHostClientFactory)
                .GetMethod(nameof(Create), new[] { typeof(HttpClient), typeof(string), typeof(IRemoteRequestHeaderProvider) })!
                .MakeGenericMethod(iface);

            return method.Invoke(null, new object[] { httpClient, routePrefix!, headerProvider! })!;
        }

        /// <summary>
        /// A dispatch proxy for creating remote API clients.
        /// </summary>
        private class HttpClientProxy : DispatchProxy
        {
            private HttpClient? _httpClient;

            private string _routePrefix = "";

            private IRemoteRequestHeaderProvider? _headerProvider;

            public void Configure(HttpClient httpClient, string routePrefix, IRemoteRequestHeaderProvider? headerProvider)
            {
                _httpClient = httpClient;

                _routePrefix = routePrefix?.TrimEnd('/') ?? "";

                _headerProvider = headerProvider;
            }

            protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
            {
                if (targetMethod == null) throw new ArgumentNullException(nameof(targetMethod));

                if (_httpClient == null) throw new InvalidOperationException("HttpClient is not configured.");

                var returnType = targetMethod.ReturnType;

                string httpMethod = GetHttpMethodFromName(targetMethod.Name);

                string route = $"{_routePrefix}/{targetMethod.Name}";

                if (returnType == typeof(Task))
                    return InvokeVoidAsync(httpMethod, route, args);

                if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    var resultType = returnType.GetGenericArguments()[0];

                    var method = typeof(HttpClientProxy)
                        .GetMethod(nameof(InvokeWithResultAsyncGeneric), BindingFlags.Instance | BindingFlags.NonPublic)!
                        .MakeGenericMethod(resultType);

                    return method.Invoke(this, new object[] { httpMethod, route, args })!;
                }

                var syncResponse = SendRequest(httpMethod, route, args).GetAwaiter().GetResult();

                syncResponse.EnsureSuccessStatusCode();

                if (returnType == typeof(void)) return null;

                var json = syncResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                return JsonSerializer.Deserialize(json, returnType, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            private string GetHttpMethodFromName(string name)
            {
                if (name.StartsWith("Get", StringComparison.OrdinalIgnoreCase))
                    return "GET";

                if (name.StartsWith("Post", StringComparison.OrdinalIgnoreCase))
                    return "POST";

                if (name.StartsWith("Put", StringComparison.OrdinalIgnoreCase))
                    return "PUT";

                if (name.StartsWith("Delete", StringComparison.OrdinalIgnoreCase))
                    return "DELETE";

                return "GET";
            }

            private void AddHeaders(HttpRequestMessage request)
            {
                if (_headerProvider == null) return;

                foreach (var (key, value) in _headerProvider.GetHeaders())
                    request.Headers.TryAddWithoutValidation(key, value);
            }

            private Task<HttpResponseMessage> SendRequest(string httpMethod, string route, object?[]? args)
            {
                var uri = (httpMethod == "GET" || httpMethod == "DELETE")
                    ? new Uri(AppendQuery(route, args), UriKind.Relative)
                    : new Uri(route, UriKind.Relative);

                var request = new HttpRequestMessage
                {
                    Method = new HttpMethod(httpMethod),

                    RequestUri = uri,

                    Content = (httpMethod == "GET" || httpMethod == "DELETE") ? null : CreateJsonContent(args)
                };

                AddHeaders(request);

                return _httpClient!.SendAsync(request);
            }

            private async Task InvokeVoidAsync(string httpMethod, string route, object?[]? args)
            {
                var response = await SendRequest(httpMethod, route, args);

                response.EnsureSuccessStatusCode();
            }

            private async Task<T> InvokeWithResultAsyncGeneric<T>(string httpMethod, string route, object?[]? args)
            {
                var response = await SendRequest(httpMethod, route, args);

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();

                return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
            }

            private string AppendQuery(string route, object?[]? args)
            {
                if (args == null || args.Length == 0 || args[0] == null) return route;

                if (args[0] is System.Collections.IDictionary dict)
                {
                    var query = string.Join("&", dict.Keys.Cast<object>().Select(k =>
                        $"{Uri.EscapeDataString(k.ToString()!)}={Uri.EscapeDataString(dict[k]?.ToString()!)}"));

                    return $"{route}?{query}";
                }

                return $"{route}?param={Uri.EscapeDataString(args[0].ToString()!)}";
            }

            private HttpContent CreateJsonContent(object?[]? args)
            {
                var obj = args != null && args.Length > 0 ? args[0] : null;

                var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                return new StringContent(json, Encoding.UTF8, "application/json");
            }
        }
    }
}
