using System.Reflection;
using System.Text.Json;
using System.Text;

namespace EasyCore.AspNetCore.Mvc.RemoteServices
{
    public static class RemoteApiK8sClientFactory
    {
        public static T Create<T>(HttpClient httpClient, IRemoteRequestHeaderProvider? headerProvider = null) where T : class
        {
            var routePrefix = GetDefaultRoutePrefix(typeof(T));

            var proxy = DispatchProxy.Create<T, HttpClientProxy>();

            (proxy as HttpClientProxy)!.Configure(httpClient, routePrefix, headerProvider);

            return proxy!;
        }

        private static string GetDefaultRoutePrefix(Type interfaceType)
        {
            var name = interfaceType.Name;

            if (name.StartsWith("I")) name = name.Substring(1);

            name = name.Replace("AppService", "").Replace("Service", "");

            return $"api/{name}";
        }

        private class HttpClientProxy : DispatchProxy
        {
            private HttpClient? _httpClient;
            private string _routePrefix = "";
            private IRemoteRequestHeaderProvider? _headerProvider;

            public void Configure(HttpClient httpClient, string routePrefix, IRemoteRequestHeaderProvider? headerProvider)
            {
                _httpClient = httpClient;
                _routePrefix = routePrefix;
                _headerProvider = headerProvider;
            }

            protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
            {
                if (targetMethod == null) throw new ArgumentNullException(nameof(targetMethod));

                if (_httpClient == null) throw new InvalidOperationException("HttpClient not configured.");

                var httpMethod = GetHttpMethodFromName(targetMethod.Name);

                var route = $"{_routePrefix}/{targetMethod.Name}";

                var returnType = targetMethod.ReturnType;

                if (!typeof(Task).IsAssignableFrom(returnType))
                {
                    var result = SendRequest(httpMethod, route, args).GetAwaiter().GetResult();

                    result.EnsureSuccessStatusCode();

                    if (returnType == typeof(void)) return null;

                    var json = result.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                    return JsonSerializer.Deserialize(json, returnType, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }

                if (returnType == typeof(Task))
                    return InvokeVoidAsync(httpMethod, route, args);

                var resultType = returnType.GetGenericArguments()[0];

                var method = typeof(HttpClientProxy).GetMethod(nameof(InvokeWithResultAsyncGeneric), BindingFlags.Instance | BindingFlags.NonPublic)!
                    .MakeGenericMethod(resultType);

                return method.Invoke(this, new object[] { httpMethod, route, args! });
            }

            private string GetHttpMethodFromName(string name) =>
                name.StartsWith("Get", StringComparison.OrdinalIgnoreCase) ? "GET" :
                name.StartsWith("Post", StringComparison.OrdinalIgnoreCase) ? "POST" :
                name.StartsWith("Put", StringComparison.OrdinalIgnoreCase) ? "PUT" :
                name.StartsWith("Delete", StringComparison.OrdinalIgnoreCase) ? "DELETE" :
                "GET";

            private async Task InvokeVoidAsync(string method, string route, object?[]? args)
            {
                var response = await SendRequest(method, route, args);

                response.EnsureSuccessStatusCode();
            }

            private async Task<T> InvokeWithResultAsyncGeneric<T>(string method, string route, object?[]? args)
            {
                var response = await SendRequest(method, route, args);

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();

                return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
            }

            private Task<HttpResponseMessage> SendRequest(string method, string route, object?[]? args)
            {
                var request = new HttpRequestMessage
                {
                    Method = new HttpMethod(method),
                    RequestUri = new Uri(route, UriKind.Relative)
                };

                if (_headerProvider != null)
                {
                    foreach (var (key, value) in _headerProvider.GetHeaders())
                    {
                        request.Headers.TryAddWithoutValidation(key, value);
                    }
                }

                if ((method == "GET" || method == "DELETE") && args?.Length > 0 && args[0] != null)
                {
                    var query = args[0] is IDictionary<string, object> dict
                        ? string.Join("&", dict.Select(kv => $"{kv.Key}={kv.Value}"))
                        : $"param={args[0]}";

                    request.RequestUri = new Uri($"{route}?{query}", UriKind.Relative);
                }
                else if (method == "POST" || method == "PUT")
                {
                    var json = JsonSerializer.Serialize(args?.FirstOrDefault());

                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                return _httpClient!.SendAsync(request);
            }
        }
    }
}
