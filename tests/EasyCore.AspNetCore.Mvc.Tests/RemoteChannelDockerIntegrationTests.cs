using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Consul;
using EasyCore.AspNetCore.Mvc.RemoteServices;
using EasyCore.AspNetCore.Mvc.RemoteServices.DaprOptions;
using EasyCore.AspNetCore.Mvc.RemoteServices.NacosOptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EasyCore.AspNetCore.Mvc.Tests;

/// <summary>
/// Live container / cluster integration tests.
/// Requires:
///   docker compose -f demo/docker-compose.remote-test.yml up -d
///   kubectl apply -f demo/k8s/easycore-provider.yaml  (Docker Desktop Kubernetes)
/// </summary>
public class RemoteChannelDockerIntegrationTests : IAsyncLifetime
{
    private static readonly HttpClient Probe = new() { Timeout = TimeSpan.FromSeconds(3) };

    private WebApplication? _providerApp;
    private int _providerPort;
    private string ProviderBase => $"http://127.0.0.1:{_providerPort}/";

    public async Task InitializeAsync()
    {
        await EnsureRegistriesReadyAsync();
        await EnsureDaprReadyAsync();
        await EnsureK8sProviderReadyAsync();

        _providerPort = GetFreeTcpPort();
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls($"http://127.0.0.1:{_providerPort}");
        builder.Logging.ClearProviders();

        _providerApp = builder.Build();
        _providerApp.MapGet("/api/ApiHostProbe/GetDto", () => Results.Json(new { name = "Hello", age = 18 }));
        _providerApp.MapGet("/api/ProviderTest/GetDto", () => Results.Json(new { name = "Hello", age = 18 }));
        _providerApp.MapGet("/api/ProviderConsulTest/GetDto", () => Results.Json(new { name = "Consul", age = 1 }));
        _providerApp.MapGet("/api/ProviderNacosTest/GetDto", () => Results.Json(new { name = "Nacos", age = 2 }));

        await _providerApp.StartAsync();
    }

    public async Task DisposeAsync()
    {
        if (_providerApp != null)
            await _providerApp.DisposeAsync();
    }

    [Fact]
    public void K8s_BuildK8sBaseAddress_Is_Production_Ready()
    {
        var uri = RemoteApiK8sClientExtensions.BuildK8sBaseAddress("provider", "default", "cluster.local", 8080);
        Assert.Equal("http://provider.default.svc.cluster.local:8080/", uri.ToString());
    }

    [Fact]
    public async Task K8s_LibraryDns_Resolves_Inside_Real_Cluster()
    {
        var baseAddress = RemoteApiK8sClientExtensions.BuildK8sBaseAddress(
            "easycore-k8s-provider",
            "default",
            "cluster.local",
            port: 80);

        Assert.Equal("http://easycore-k8s-provider.default.svc.cluster.local/", baseAddress.ToString());

        var url = new Uri(baseAddress, "api/ProviderK8sTest/GetDto").ToString();
        var podName = $"easycore-k8s-probe-{Guid.NewGuid():N}"[..40];

        try
        {
            var (runExit, runOut, runErr) = await RunProcessAsync(
                "kubectl",
                $"run {podName} --restart=Never --image=curlimages/curl:8.5.0 -- " +
                $"curl -sf --max-time 20 {url}",
                TimeSpan.FromSeconds(60));
            Assert.True(runExit == 0, $"kubectl run failed ({runExit}). stdout={runOut} stderr={runErr}");

            var (waitExit, waitOut, waitErr) = await RunProcessAsync(
                "kubectl",
                $"wait --for=jsonpath={{.status.phase}}=Succeeded pod/{podName} --timeout=120s",
                TimeSpan.FromSeconds(130));
            Assert.True(waitExit == 0, $"kubectl wait failed ({waitExit}). stdout={waitOut} stderr={waitErr}");

            var (logExit, logOut, logErr) = await RunProcessAsync(
                "kubectl",
                $"logs {podName}",
                TimeSpan.FromSeconds(30));
            Assert.True(logExit == 0, $"kubectl logs failed ({logExit}). stdout={logOut} stderr={logErr}");

            using var doc = JsonDocument.Parse(logOut.Trim());
            Assert.Equal("K8s", doc.RootElement.GetProperty("name").GetString());
            Assert.Equal(42, doc.RootElement.GetProperty("age").GetInt32());
        }
        finally
        {
            await RunProcessAsync(
                "kubectl",
                $"delete pod {podName} --ignore-not-found --wait=false",
                TimeSpan.FromSeconds(30));
        }
    }

    [Fact]
    public async Task ApiHost_Proxy_Calls_Local_Provider()
    {
        using var handler = new HttpClientHandler();
        using var http = new HttpClient(handler) { BaseAddress = new Uri(ProviderBase) };
        var proxy = RemoteApiHostClientFactory.Create<IApiHostProbe>(http);
        var dto = await proxy.GetDto();
        Assert.Equal("Hello", dto.Name);
        Assert.Equal(18, dto.Age);
    }

    [Fact]
    public async Task Consul_Discovery_And_Handler_Reach_Provider()
    {
        using var consul = new ConsulClient(c => c.Address = new Uri("http://127.0.0.1:8500"));
        var serviceId = $"provider-test-{Guid.NewGuid():N}";
        await consul.Agent.ServiceRegister(new AgentServiceRegistration
        {
            ID = serviceId,
            Name = "ProviderIntegration",
            Address = "127.0.0.1",
            Port = _providerPort
        });

        try
        {
            await WaitUntilAsync(async () =>
            {
                var result = await consul.Health.Service("ProviderIntegration", string.Empty, passingOnly: true);
                return result.Response is { Length: > 0 };
            }, TimeSpan.FromSeconds(30));

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<IConsulClient>(consul);
            services.AddSingleton<ConsulServiceDiscovery>();
            await using var sp = services.BuildServiceProvider();
            var discovery = sp.GetRequiredService<ConsulServiceDiscovery>();

            var baseUri = await discovery.GetServiceUriAsync("ProviderIntegration");
            Assert.NotNull(baseUri);
            Assert.Equal(ProviderBase, baseUri!.ToString());

            using var http = new HttpClient(new ConsulResolvingHandler(discovery, "ProviderIntegration")
            {
                InnerHandler = new HttpClientHandler()
            })
            {
                BaseAddress = new Uri("http://consul.placeholder/")
            };

            var json = await http.GetStringAsync("api/ProviderTest/GetDto");
            using var doc = JsonDocument.Parse(json);
            Assert.Equal("Hello", doc.RootElement.GetProperty("name").GetString());
        }
        finally
        {
            await consul.Agent.ServiceDeregister(serviceId);
        }
    }

    [Fact]
    public async Task Nacos_Discovery_And_Handler_Reach_Provider()
    {
        var serviceName = $"ProviderIntegration-{Guid.NewGuid():N}"[..32];
        using var registerClient = new HttpClient { BaseAddress = new Uri("http://127.0.0.1:8848/") };

        var registerUrl =
            "nacos/v1/ns/instance" +
            $"?serviceName={Uri.EscapeDataString(serviceName)}" +
            "&groupName=DEFAULT_GROUP" +
            "&ip=127.0.0.1" +
            $"&port={_providerPort}" +
            "&healthy=true" +
            "&enabled=true" +
            "&ephemeral=true";

        using (var registerResponse = await registerClient.PostAsync(registerUrl, content: null))
        {
            registerResponse.EnsureSuccessStatusCode();
        }

        try
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddHttpClient(NacosServiceDiscovery.HttpClientName);
            services.AddSingleton<IOptionsMonitor<NacosOption>>(
                new TestOptionsMonitor<NacosOption>(new NacosOption
                {
                    ServerAddresses = "http://127.0.0.1:8848",
                    GroupName = "DEFAULT_GROUP"
                }));
            services.AddSingleton<NacosServiceDiscovery>();
            await using var sp = services.BuildServiceProvider();
            var discovery = sp.GetRequiredService<NacosServiceDiscovery>();

            Uri? baseUri = null;
            await WaitUntilAsync(async () =>
            {
                baseUri = await discovery.GetServiceUriAsync(serviceName);
                return baseUri != null;
            }, TimeSpan.FromSeconds(45));

            Assert.NotNull(baseUri);
            Assert.Equal(ProviderBase, baseUri!.ToString());

            using var http = new HttpClient(new NacosResolvingHandler(discovery, serviceName, "DEFAULT_GROUP")
            {
                InnerHandler = new HttpClientHandler()
            })
            {
                BaseAddress = new Uri("http://nacos.placeholder/")
            };

            var json = await http.GetStringAsync("api/ProviderNacosTest/GetDto");
            using var doc = JsonDocument.Parse(json);
            Assert.Equal("Nacos", doc.RootElement.GetProperty("name").GetString());
        }
        finally
        {
            var deregisterUrl =
                "nacos/v1/ns/instance" +
                $"?serviceName={Uri.EscapeDataString(serviceName)}" +
                "&groupName=DEFAULT_GROUP" +
                "&ip=127.0.0.1" +
                $"&port={_providerPort}" +
                "&ephemeral=true";
            await registerClient.DeleteAsync(deregisterUrl);
        }
    }

    [Fact]
    public async Task Dapr_RealSidecar_Invoke_Reaches_Provider_Container()
    {
        using var http = new HttpClient(new DaprResolvingHandler("easycore-provider")
        {
            InnerHandler = new HttpClientHandler()
        })
        {
            BaseAddress = new Uri("http://127.0.0.1:3500/")
        };

        var json = await http.GetStringAsync("api/ProviderDaprTest/GetDto");
        using var doc = JsonDocument.Parse(json);
        Assert.Equal("Dapr", doc.RootElement.GetProperty("name").GetString());
        Assert.Equal(42, doc.RootElement.GetProperty("age").GetInt32());
    }

    [Fact]
    public void Dapr_ResolveHttpEndpoint_Honors_DaprHttpEndpoint_Env()
    {
        var previous = Environment.GetEnvironmentVariable("DAPR_HTTP_ENDPOINT");
        try
        {
            Environment.SetEnvironmentVariable("DAPR_HTTP_ENDPOINT", "http://127.0.0.1:3999");
            var option = new DaprOption { HttpEndpoint = string.Empty };
            Assert.Equal("http://127.0.0.1:3999/", option.ResolveHttpEndpoint().ToString());
        }
        finally
        {
            Environment.SetEnvironmentVariable("DAPR_HTTP_ENDPOINT", previous);
        }
    }

    [Fact]
    public void Consul_BuildInstanceUri_Falls_Back_Supports_IPv6()
    {
        Assert.Equal("http://10.0.0.8:8080/", ConsulServiceDiscovery.BuildInstanceUri("10.0.0.8", 8080).ToString());
        Assert.Equal("http://[2001:db8::1]:8080/", ConsulServiceDiscovery.BuildInstanceUri("2001:db8::1", 8080).ToString());
    }

    private static async Task EnsureRegistriesReadyAsync()
    {
        await WaitUntilAsync(async () =>
        {
            try
            {
                using var response = await Probe.GetAsync("http://127.0.0.1:8500/v1/status/leader");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }, TimeSpan.FromSeconds(60), "Consul is not reachable on :8500. Start demo/docker-compose.remote-test.yml");

        await WaitUntilAsync(async () =>
        {
            try
            {
                using var response = await Probe.GetAsync("http://127.0.0.1:8848/nacos/");
                return response.StatusCode is HttpStatusCode.OK or HttpStatusCode.Found or HttpStatusCode.MovedPermanently;
            }
            catch
            {
                return false;
            }
        }, TimeSpan.FromSeconds(120), "Nacos is not reachable on :8848. Start demo/docker-compose.remote-test.yml");
    }

    private static async Task EnsureDaprReadyAsync()
    {
        await WaitUntilAsync(async () =>
        {
            try
            {
                using var response = await Probe.GetAsync("http://127.0.0.1:3500/v1.0/healthz");
                // Dapr returns 204 No Content when healthy.
                return response.StatusCode is HttpStatusCode.NoContent or HttpStatusCode.OK;
            }
            catch
            {
                return false;
            }
        }, TimeSpan.FromSeconds(90), "Dapr sidecar is not reachable on :3500. Start demo/docker-compose.remote-test.yml");

        // Warm invoke path once (sidecar may need a moment after healthz).
        await WaitUntilAsync(async () =>
        {
            try
            {
                using var response = await Probe.GetAsync(
                    "http://127.0.0.1:3500/v1.0/invoke/easycore-provider/method/api/ProviderDaprTest/GetDto");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }, TimeSpan.FromSeconds(60), "Dapr invoke to easycore-provider failed.");
    }

    private static async Task EnsureK8sProviderReadyAsync()
    {
        var manifest = FindRepoFile(Path.Combine("demo", "k8s", "easycore-provider.yaml"));
        var (applyExit, applyOut, applyErr) = await RunProcessAsync(
            "kubectl",
            $"apply -f \"{manifest}\"",
            TimeSpan.FromSeconds(60));
        Assert.True(applyExit == 0, $"kubectl apply failed: {applyOut}{applyErr}");

        var (waitExit, waitOut, waitErr) = await RunProcessAsync(
            "kubectl",
            "rollout status deployment/easycore-k8s-provider --timeout=120s",
            TimeSpan.FromSeconds(130));
        Assert.True(waitExit == 0, $"kubectl rollout failed: {waitOut}{waitErr}");
    }

    private static string FindRepoFile(string relativePath)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, relativePath);
            if (File.Exists(candidate))
                return candidate;

            candidate = Path.Combine(dir.FullName, "..", "..", "..", "..", "..", relativePath);
            candidate = Path.GetFullPath(candidate);
            if (File.Exists(candidate))
                return candidate;

            dir = dir.Parent;
        }

        throw new FileNotFoundException($"Could not locate {relativePath} from {AppContext.BaseDirectory}");
    }

    private static async Task<(int ExitCode, string StdOut, string StdErr)> RunProcessAsync(
        string fileName,
        string arguments,
        TimeSpan timeout)
    {
        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi)
            ?? throw new InvalidOperationException($"Failed to start {fileName}");

        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();
        using var cts = new CancellationTokenSource(timeout);
        try
        {
            await process.WaitForExitAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            try { process.Kill(entireProcessTree: true); } catch { /* ignore */ }
            throw new TimeoutException($"{fileName} {arguments} timed out after {timeout}.");
        }

        return (process.ExitCode, await stdoutTask, await stderrTask);
    }

    private static async Task WaitUntilAsync(Func<Task<bool>> condition, TimeSpan timeout, string? failMessage = null)
    {
        var start = DateTime.UtcNow;
        while (DateTime.UtcNow - start < timeout)
        {
            if (await condition())
                return;
            await Task.Delay(1000);
        }

        throw new TimeoutException(failMessage ?? $"Condition not met within {timeout}.");
    }

    private static int GetFreeTcpPort()
    {
        var listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    public interface IApiHostProbe : IRemoteAppService
    {
        Task<ProbeDto> GetDto();
    }

    public sealed class ProbeDto
    {
        public string? Name { get; set; }
        public int Age { get; set; }
    }

    private sealed class TestOptionsMonitor<T> : IOptionsMonitor<T>
    {
        public TestOptionsMonitor(T current) => CurrentValue = current;
        public T CurrentValue { get; }
        public T Get(string? name) => CurrentValue;
        public IDisposable? OnChange(Action<T, string?> listener) => null;
    }
}
