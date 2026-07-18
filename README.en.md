# 🎯 EasyCore.AspNetCore.Mvc

> **EasyCore.AspNetCore.Mvc** is a production-oriented ASP.NET Core application-service library for .NET 8. It provides **dynamic APIs** (AppServices become controllers), **EasyCoreAppService context helpers**, and **ApiHost / Consul / Kubernetes / Nacos / Dapr** remote HTTP proxies powered by interface-level `DispatchProxy`.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![C#](https://img.shields.io/badge/C%23-12-239120?logo=csharp)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-MVC-blueviolet)
![Remote](https://img.shields.io/badge/Remote-ApiHost%20%7C%20Consul%20%7C%20K8s%20%7C%20Nacos%20%7C%20Dapr-orange)
![Mapster](https://img.shields.io/badge/Mapper-Mapster-green)
![License](https://img.shields.io/badge/License-MIT-yellow)
![Version](https://img.shields.io/badge/Version-8.3.0-blue)
![NuGet](https://img.shields.io/nuget/v/EasyCore.AspNetCore.Mvc?label=NuGet)

---

## 🌍 Language

- Chinese: [README.md](https://github.com/RockyWang0521/EasyCore.AspNetCore.Mvc/blob/master/README.md)
- **English (this document)**

---

## 📚 Table of Contents

### Part I — Overview & Architecture
- [1. Positioning](#1-positioning)
- [2. Architecture](#2-architecture)
- [3. NuGet Packages](#3-nuget-packages)
- [4. Remote Channel Comparison](#4-remote-channel-comparison)

### Part II — Getting Started
- [5. Requirements](#5-requirements)
- [6. Installation](#6-installation)
- [7. Quick Start (3 minutes)](#7-quick-start-3-minutes)
- [8. Dynamic API & Naming](#8-dynamic-api--naming)

### Part III — Remote Proxies
- [9. ApiHost (configured base address)](#9-apihost-configured-base-address)
- [10. Consul (service discovery)](#10-consul-service-discovery)
- [11. Kubernetes DNS](#11-kubernetes-dns)
- [12. Nacos (service discovery)](#12-nacos-service-discovery)
- [13. Dapr (sidecar invoke)](#13-dapr-sidecar-invoke)
- [14. EasyCoreAppService Helpers](#14-easycoreappservice-helpers)
- [15. Remote Auth & Header Forwarding](#15-remote-auth--header-forwarding)

### Part IV — Demo & Production
- [16. Demo Projects](#16-demo-projects)
- [17. Migrating from older versions](#17-migrating-from-older-versions)
- [18. Production Checklist](#18-production-checklist)
- [19. FAQ](#19-faq)
- [20. License](#20-license)

---

## 1. Positioning

EasyCore.AspNetCore.Mvc reduces controller boilerplate and makes remote calls feel local:

| Pain point | EasyCore.AspNetCore.Mvc approach |
|---|---|
| Hand-written controllers | `EasyCoreAppService` + `AddEasyCoreDynamicApi()` |
| Scattered `HttpClient` usage | Interface + attribute → `DispatchProxy` |
| Provider replaced by remote proxy | Skip registration when a local implementation exists |
| Hard to forward Token / tenant | `IRemoteRequestHeaderProvider` |
| Heavy mapper stack | Built-in **Mapster** (`Mapper`) |

### 1.1 Design Principles

| Principle | Meaning |
|---|---|
| **Low friction** | A few extension methods + a base class |
| **Interface-only proxies** | Remote side never activates concrete AppServices |
| **Pluggable channels** | ApiHost / Consul / K8s / Nacos / Dapr are opt-in |
| **Convention over configuration** | Method-name prefixes imply HTTP verbs |
| **Ecosystem-friendly** | Works with `EasyCore.Dependency` and `EasyCore.Consul` |

### 1.2 Repository Layout

```text
EasyCore.AspNetCore.Mvc/
├── src/EasyCore.AspNetCore.Mvc/     # Core: DynamicApi / AppService / Remote
├── demo/
│   ├── Provider/                   # :5094 — Dynamic API + EFCore + Consul
│   └── Consumer/                   # :5244 — ApiHost / Consul clients
├── tests/EasyCore.AspNetCore.Mvc.Tests/
├── docs/svg/                       # README diagrams
└── png/EasyCoreLogo.png
```

---

## 2. Architecture

### 2.1 Component Diagram

![architecture-en](https://raw.githubusercontent.com/RockyWang0521/EasyCore.AspNetCore.Mvc/master/docs/svg/architecture-en.svg)

### 2.2 Remote Call Lifecycle

![sequence-en](https://raw.githubusercontent.com/RockyWang0521/EasyCore.AspNetCore.Mvc/master/docs/svg/sequence-en.svg)

### 2.3 Data Flow

```text
[EasyCoreAppService]
        │
        ▼
 AddEasyCoreDynamicApi ──► MVC Controller ──► Swagger
        │
        │   (Consumer side)
        ▼
 [ApiHost|Consul|K8s|Nacos|Dapr] Attribute
        │
        ▼
 FindRemoteInterfaces ──► DispatchProxy ──► HttpClient
        │                                      │
        └──── Local impl? skip ◄───────────────┘
```

---

## 3. NuGet Packages

| Package | Role | Required |
|---|---|---|
| `EasyCore.AspNetCore.Mvc` | Dynamic API, AppService, remote proxies | ✅ |
| `EasyCore.Dependency` | Auto DI via `ITransientDependency` | Recommended |
| `EasyCore.Consul` | Registration / KV / locks (Consul channel) | Optional |
| `EasyCore.EFCoreRepository` | Demo / business repositories | Optional |

---

## 4. Remote Channel Comparison

| Capability | ApiHost | Consul | Kubernetes | Nacos | Dapr |
|---|---|---|---|---|---|
| Attribute | `[ApiHost("Name")]` | `[ConsulService("Name")]` | `[K8sDns("svc", port)]` | `[NacosService("Name")]` | `[DaprApp("app-id")]` |
| Registration | `AddEasyCoreRemoteApiClients()` | `AddEasyCoreRemoteApiConsulClients()` | `AddEasyCoreRemoteApiK8sClients(...)` | `AddEasyCoreRemoteApiNacosClients()` | `AddEasyCoreRemoteApiDaprClients()` |
| Address source | `RemoteServices:Name` | Consul Health API | DNS: `{svc}.{ns}.svc.{domain}` | Naming OpenAPI instance list | Local sidecar HTTP invoke |
| Typical use | Local / fixed gateway | Multi-instance discovery | In-cluster Service | Common CN registry | Sidecar mesh / polyglot |
| Extra dependency | None | Consul Agent | Cluster DNS | Nacos Server | Dapr sidecar |

### 4.1 Decision Tree

```text
How do you obtain the remote address?
├── Fixed BaseAddress in config → ApiHost
├── Consul registry → Consul
├── Nacos registry → Nacos
├── Already inside Kubernetes → K8sDns
└── Dapr sidecar injected → Dapr
```

---

## 5. Requirements

| Item | Requirement |
|---|---|
| .NET | 8.0+ |
| Host | ASP.NET Core (Web / API) |
| DI | `EasyCore.Dependency` 8.0.1+ recommended |
| Consul | Optional; required for the Consul channel |
| Nacos | Optional; Naming OpenAPI must be reachable |
| Dapr | Optional; local sidecar HTTP port required |
| K8s | Optional; in-cluster DNS only |

---

## 6. Installation

```bash
dotnet add package EasyCore.AspNetCore.Mvc
dotnet add package EasyCore.Dependency

# when using the Consul channel
dotnet add package EasyCore.Consul
```

---

## 7. Quick Start (3 minutes)

### 7️⃣.1️⃣ Define an AppService

```csharp
using EasyCore.AspNetCore.Mvc.AppService;
using EasyCore.Dependency;

public class ProviderTestAppService : EasyCoreAppService, IProviderTestAppService
{
    public Task<PostDto> GetDto()
        => Task.FromResult(new PostDto { Name = "Hello", Age = 18 });

    public Task PostDto(PostDto dto) => Task.CompletedTask;
}
```

### 7️⃣.2️⃣ Register

```csharp
using EasyCore.AspNetCore.Mvc.AppService;
using EasyCore.AspNetCore.Mvc.DynamicApi;
using EasyCore.Dependency;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddEasyCoreDynamicApi();
builder.Services.AddEasyCoreAppServices();
builder.Services.AddEasyCoreDependency();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapControllers();
app.Run();
```

### 7️⃣.3️⃣ Preview

![dynamic-api-preview](https://raw.githubusercontent.com/RockyWang0521/EasyCore.AspNetCore.Mvc/master/docs/svg/dynamic-api-preview.svg)

Open Swagger to see `GET/POST/PUT/DELETE` generated from method names.

---

## 8. Dynamic API & Naming

| Rule | Description |
|---|---|
| Base type | Inherit `EasyCoreAppService` (or `BaseAppService`) |
| Route | Default `api/[controller]/[action]` |
| Verb | Method name starts with `Get` / `Post` / `Put` / `Delete` |
| Remote parity | Proxies use the same verb inference |
| Unknown verb | Proxy throws (no silent fallback) |

> ⚠️ Avoid method names without a verb prefix; remote proxies cannot infer the HTTP method.

---

## 9. ApiHost (configured base address)

### 9️⃣.1️⃣ Contract

```csharp
using EasyCore.AspNetCore.Mvc.RemoteServices;
using EasyCore.Dependency;

[ApiHost("Provider")]
public interface IProviderTestAppService : IRemoteAppService, ITransientDependency
{
    Task<PostDto> GetDto();
    Task PostDto(PostDto dto);
    Task PutDto(PostDto dto);
    Task DeleteDto(Guid id);
}
```

### 9️⃣.2️⃣ Configuration

```json
{
  "RemoteServices": {
    "Provider": "http://localhost:5094"
  }
}
```

### 9️⃣.3️⃣ Consumer registration

```csharp
builder.Services.AddEasyCoreRemoteApiClients();

public class ConsumerTestAppService(
    IProviderTestAppService provider) : EasyCoreAppService
{
    public Task<PostDto> GetRemoteApi() => provider.GetDto();
}
```

If the host already has a local implementation, the remote proxy is **not** registered.

---

## 10. Consul (service discovery)

### 🔟.1️⃣ Contract

```csharp
[ConsulService("Provider")]
public interface IProviderConsulTestAppService : IRemoteAppService, ITransientDependency
{
    Task<PostDto> GetDto();
}
```

### 🔟.2️⃣ Register the Provider

```csharp
using EasyCore.Consul;

builder.AddEasyCoreConsul()
    .AddEasyCoreConsulCache()
    .AddEasyCoreConsulLocking()
    .AddEasyCoreConsulServer();

var app = builder.Build();
app.UseEasyCoreConsul(); // maps /healthCheck; registration via HostedService
```

Sample `appsettings.json`:

```json
{
  "Consul": {
    "ServiceName": "Provider",
    "ServiceIP": "localhost",
    "ServicePort": 5094,
    "ServiceHealthCheck": "http://localhost:5094/healthCheck",
    "ConsulAddress": "http://localhost:8500/"
  }
}
```

### 🔟.3️⃣ Enable Consul clients on the Consumer

```csharp
builder.Services.AddEasyCoreRemoteApiConsulClients();
builder.AddEasyCoreConsul()
    .AddEasyCoreConsulCache()
    .AddEasyCoreConsulLocking()
    .AddEasyCoreConsulServer();
```

Resolution path: `Health.Service` → pick a healthy instance → `ConsulResolvingHandler` rewrites the request URI.

---

## 11. Kubernetes DNS

```csharp
builder.Services.AddEasyCoreRemoteApiK8sClients(options =>
{
    options.K8sNamespace = "default";
    // Final host: {ServiceName}.default.svc.cluster.local
    options.K8sClusterDomain = "cluster.local";
});
```

```csharp
[K8sDns("provider")]           // port 80 omitted
[K8sDns("provider", 8080)]     // non-default port
public interface IProviderK8sAppService : IRemoteAppService, ITransientDependency
{
    Task<PostDto> GetDto();
}
```

BaseAddress format: `http://{service}.{namespace}.svc.{clusterDomain}[:port]/`  
Legacy `ClusterDomain = "svc.cluster.local"` is normalized automatically.

---

## 12. Nacos (service discovery)

Uses Nacos Naming OpenAPI (**no** `nacos-sdk-csharp`) to pick healthy instances; `NacosResolvingHandler` rewrites the request URI. Authorization and related headers are forwarded by `HttpContextHeaderProvider`.

### 12.1 Contract

```csharp
[NacosService("Provider")]                 // optional group as 2nd argument
public interface IProviderNacosTestAppService : IRemoteAppService, ITransientDependency
{
    Task<PostDto> GetDto();
}
```

### 12.2 Configuration

```json
{
  "Nacos": {
    "ServerAddresses": "http://localhost:8848",
    "Namespace": "",
    "GroupName": "DEFAULT_GROUP"
  }
}
```

Optional: `UserName` / `Password` for OpenAPI auth.

### 12.3 Consumer registration

```csharp
builder.Services.AddEasyCoreRemoteApiNacosClients();
```

Resolution path: `GET /nacos/v1/ns/instance/list?healthyOnly=true` → random healthy instance → `http://ip:port/`.

> This library performs **discovery and invocation only**; it does not register services into Nacos.

---

## 13. Dapr (sidecar invoke)

Uses the local Dapr HTTP sidecar service invocation API (**no** `Dapr.Client`). Headers are forwarded the same way.

### 13.1 Contract

```csharp
[DaprApp("provider")]   // Dapr app-id
public interface IProviderDaprTestAppService : IRemoteAppService, ITransientDependency
{
    Task<PostDto> GetDto();
}
```

### 13.2 Configuration

```json
{
  "Dapr": {
    "HttpEndpoint": "http://127.0.0.1:3500/"
  }
}
```

When empty: read `DAPR_HTTP_PORT`, otherwise default to `http://127.0.0.1:3500/`.

### 13.3 Consumer registration

```csharp
builder.Services.AddEasyCoreRemoteApiDaprClients();
```

Path rewrite: `api/Foo/GetDto` → `/v1.0/invoke/provider/method/api/Foo/GetDto`.

---

## 14. EasyCoreAppService Helpers

```csharp
public async Task GetContextInfo()
{
    var tenant = CurrentTenant;          // X-Tenant-Id
    var user = CurrentUser.UserName;     // Claims
    var guid = GuidFactory.NewGuid;      // sequential GUID
    var token = CurrentToken.RequestToken;
    var dto = Mapper.Map<PostDto>(entity); // Mapster

    await Task.CompletedTask;
}
```

| Member | Purpose |
|---|---|
| `CurrentUser` | Authenticated user claims |
| `CurrentTenant` | Tenant id (`X-Tenant-Id`) |
| `CurrentToken` | Bearer token from the request |
| `GuidFactory` | Sequential GUIDs |
| `Mapper` | Mapster `IMapper` |

Remote proxies forward Token / tenant headers via `HttpContextHeaderProvider` when present.

---

## 15. Remote Auth & Header Forwarding

When the remote API requires authentication, the library **does not** log in or refresh tokens. It forwards inbound request headers:

| Header | Purpose |
|---|---|
| `Authorization` | Including `Bearer xxx` |
| `X-Tenant-Id` | Tenant |
| `X-Trace-Id` | Tracing |

Requires an `HttpContext` from an inbound HTTP request. Background jobs without a context will not attach auth headers.  
For custom headers (for example `X-Api-Key`), implement `IRemoteRequestHeaderProvider` and replace the DI registration.

---

## 16. Demo Projects

![demo-topology-en](https://raw.githubusercontent.com/RockyWang0521/EasyCore.AspNetCore.Mvc/master/docs/svg/demo-topology-en.svg)

| Project | Port | Role | Command |
|---|---|---|---|
| [`Provider.Host`](demo/Provider/Provider.Host) | 5094 | Dynamic API + EFCore + Consul | `dotnet run --project demo/Provider/Provider.Host` |
| [`Consumer.Host`](demo/Consumer/Consumer.Host) | 5244 | ApiHost / Consul remote calls | `dotnet run --project demo/Consumer/Consumer.Host` |
| Consul (optional) | 8500 | Discovery UI | see `demo/docker-compose.consul.yml` or `demo/docker-compose.remote-test.yml` |
| Nacos (optional) | 8848 | Naming integration tests | `docker compose -f demo/docker-compose.remote-test.yml up -d` |
| Dapr (optional) | 3500 | Real sidecar invoke | same compose (`easycore-daprd`) |
| K8s (optional) | — | Docker Desktop cluster DNS | `kubectl apply -f demo/k8s/easycore-provider.yaml` |

```bash
# 1. (optional) start Consul
docker compose -f demo/docker-compose.consul.yml up -d

# 2. start Provider
dotnet run --project demo/Provider/Provider.Host
# Swagger: http://localhost:5094/swagger

# 3. start Consumer
dotnet run --project demo/Consumer/Consumer.Host
# Swagger: http://localhost:5244/swagger
```

---

## 17. Migrating from older versions

| Older | Current (8.3.0) |
|---|---|
| `EasyCore.Dependencie` / `ITransientDependencie` | `EasyCore.Dependency` / `ITransientDependency` |
| `EasyCoreDependencie()` / `EasyCoreDependency()` | `AddEasyCoreDependency()` |
| `builder.EasyCoreConsul(args)...` | `builder.AddEasyCoreConsul()...` |
| AutoMapper | **Mapster** |
| Remote proxy activated concrete types | **Interface-only** DispatchProxy |
| K8s `ClusterDomain = "svc.cluster.local"` | Prefer `"cluster.local"` (legacy accepted) |
| Demo `App1` / `App2` | `Provider` / `Consumer` |

---

## 18. Production Checklist

- [ ] Keep contracts in a Contracts assembly to avoid circular references
- [ ] Register Provider vs Consumer features per environment (avoid proxy loops)
- [ ] Store `RemoteServices` / Consul / Nacos / Dapr endpoints in a config store; no hard-coded secrets
- [ ] Consul: advertise a reachable `ServiceAddress`; keep health checks green
- [ ] Nacos: verify service name, group, namespace, and healthy instances
- [ ] Dapr: verify app-id and sidecar HTTP port; do not expose an unauthenticated sidecar publicly
- [ ] K8s: verify Service name, namespace, port, and cluster domain
- [ ] Follow Get/Post/Put/Delete method-name prefixes
- [ ] Do not expose unauthenticated admin surfaces publicly
- [ ] Run `dotnet test` in CI

---

## 19. FAQ

**Q: AppService missing from Swagger?**  
A: Inherit `EasyCoreAppService` and call `AddEasyCoreDynamicApi()` plus `AddEasyCoreDependency()` (or equivalent DI registration).

**Q: Consumer resolves a local type / missing dependencies?**  
A: Since 8.0, proxies register only when **no local implementation** exists. Do not reference the Provider implementation project from Consumer.

**Q: Consul calls fail?**  
A: Ensure the service name matches `[ConsulService("...")]`, health checks pass, and `ConsulAddress` is reachable. A placeholder BaseAddress is rewritten by `ConsulResolvingHandler`.

**Q: Nacos finds no instances?**  
A: Check `Nacos:ServerAddresses`, service name / group / namespace, and whether instances are healthy. This library does not register services.

**Q: Dapr invoke fails?**  
A: Ensure the sidecar is running, `Dapr:HttpEndpoint` or `DAPR_HTTP_PORT` is correct, and `[DaprApp]` matches the peer app-id.

**Q: K8s DNS fails?**  
A: Confirm the Pod can resolve `service.namespace.svc.cluster.local` and the port matches. Use `cluster.local` as `K8sClusterDomain`.

**Q: Is the Token forwarded automatically?**  
A: Yes, when Authorization / tenant headers exist on the current request. See [§15](#15-remote-auth--header-forwarding).

---

## 20. License

MIT — see [LICENSE](LICENSE) and the NuGet package declaration.

---

## 🤝 Contributing

1. Fork and create a feature branch  
2. Add tests under `tests/EasyCore.AspNetCore.Mvc.Tests`  
3. Run `dotnet test` and build the solution  
4. Open a Pull Request / Merge Request  

Repository: [github.com/RockyWang0521/EasyCore.AspNetCore.Mvc](https://github.com/RockyWang0521/EasyCore.AspNetCore.Mvc)  

Issues and PRs are welcome 🚀
