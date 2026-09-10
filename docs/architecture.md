# Maankix architecture

Status: foundation checkpoint  
Audience: endpoint, kernel, and platform engineers

## 1. Product shape

Maankix is an enterprise Windows endpoint DLP platform. The long-term control plane is:

```
Windows Kernel MiniFilter
        ↓
Windows Endpoint Service (.NET 8)
        ↓
Backend
        ↓
Knowledge Graph
        ↓
Behavior Intelligence
```

Each arrow is a trust and reliability boundary. User-mode code must not assume the filter is present. The filter must not assume the service is healthy. The service must not assume the backend is reachable.

This document describes the **intended** system and the **implemented** slice. The endpoint service host and the vendored MiniSpy minifilter (`maankixflt`) exist. They are not connected.

## 2. What this checkpoint ships

Implemented:

- Repository layout, solution, and shared build (`Directory.Build.props`, `Directory.Packages.props`, `.editorconfig`, `global.json`)
- `Maankix.Endpoint.Service` — .NET 8 Worker Service that can run interactively or under Service Control Manager
- Lightweight Clean Architecture folders inside the service
- Dependency injection, `IConfiguration`, options validation, structured logging
- Empty ports for later product work (except the filter port, which is now implemented)
- Architecture decision records
- `maankixflt` — Microsoft MiniSpy vendored as `src/Maankix.Endpoint.Driver` (see [driver.md](driver.md))
- `MiniSpyDriverClient` — same protocol as MiniSpy.exe (`\MiniSpyPort`, `GetMiniSpyLog`)

Not implemented (explicitly forbidden here):

- Policy model or enforcement
- Blocking (`FLT_PREOP_COMPLETE` / access denied)
- Policy model or enforcement
- Backend protocol or REST
- SQLite or any persistence
- Knowledge graph
- Behavior intelligence / AI

## 3. Endpoint service

The Windows Endpoint Service is a long-running user-mode process. It is the only process that will later talk to both the kernel filter and the control plane.

```
Program.cs                         composition root
        │
        ▼
Configuration/                     host, logging, DI, options
        │
        ├── Worker/                IHostedService lifetime
        ├── Application/           use-case ports (empty)
        ├── Infrastructure/        adapters (TimeProvider only)
        └── Domain/                enterprise ports (empty)
```

### 3.1 Layer rules

| Layer | Allowed references | Forbidden |
| --- | --- | --- |
| Domain | None of the other Maankix layers | Infrastructure, Worker, Configuration, Microsoft.Extensions.DependencyInjection |
| Application | Domain | Infrastructure, Worker |
| Infrastructure | Domain | Application (unless a future adapter needs a use-case port; prefer Domain ports) |
| Worker | Domain, Application, Configuration | Embedding domain rules |
| Configuration | All layers | Business decisions |
| Common | None | Layer-specific types |

`Program.cs` may only construct the host and call `ConfigureMaankixEndpoint`.

These rules are convention in a single project (see [ADR-001](adr/ADR-001-lightweight-clean-architecture.md)). Split assemblies when a second product surface (installer, filter user-mode helper, or test harness) needs a hard reference boundary.

### 3.2 Ports reserved for later checkpoints

| Port | Layer | Future owner |
| --- | --- | --- |
| `IEndpointIdentity` | Domain | Infrastructure adapter that reads machine / agent identity |
| `IFilterCommunicationPort` | Domain | Infrastructure adapter over Filter Manager port APIs |
| `IEndpointModule` | Application | Feature modules registered at composition time |

Ports have no members. Adding a method is a later, reviewed change.

### 3.3 Hosting

See [ADR-002](adr/ADR-002-dotnet8-worker-windows-service.md).

- Generic Host (`Host.CreateApplicationBuilder`)
- `AddWindowsService` for SCM lifetime
- Content root set to `AppContext.BaseDirectory` when running as a Windows Service
- `EndpointWorker` waits on the stopping token; it does not poll a product queue yet
- `HostOptions.ShutdownTimeout` is bound from `Maankix:Endpoint:ShutdownTimeout`

### 3.4 Configuration

```
Maankix:Endpoint:ServiceName        log identity
Maankix:Endpoint:ShutdownTimeout    host stop timeout
Logging:*                           Microsoft.Extensions.Logging
```

Options are validated at startup (`ValidateOnStart`). Invalid configuration fails the process before the worker starts.

### 3.5 Logging

Structured logging is a host concern:

- JSON console in non-Development environments (timestamp UTC, scopes enabled via configuration)
- Simple console in Development (`appsettings.Development.json`)
- Windows Event Log (`Application` / `Maankix Endpoint Service`)
- Debug provider for attached debugger sessions

Log property names use PascalCase (`ServiceName={ServiceName}`) so they survive JSON formatting without abbreviation.

## 4. Future component map

The following components are planned, not present. They are listed so later checkpoints land in the correct layer instead of growing `Program.cs`.

| Component | Intended home | Notes |
| --- | --- | --- |
| Minifilter driver | `src/Maankix.Endpoint.Driver` (`maankixflt`) | MiniSpy foundation; compile / load / unload only |
| Filter port adapter | Infrastructure `MiniSpyDriverClient` | Implements `IDriverClient` / `IFilterCommunicationPort` |
| Policy snapshot / evaluator | Domain + Application | No evaluator until a policy model exists |
| Local store | Infrastructure | SQLite is a later decision; do not leak it into Domain |
| Control-plane client | Infrastructure | Transport is a later decision |
| Knowledge graph / BI | Backend repositories | Out of the endpoint process |

## 5. Trust and process model (target)

```
┌─────────────────────────────────────────────┐
│  Kernel                                    │
│  Maankix minifilter (not in this repo yet) │
└────────────────────┬────────────────────────┘
                     │ Filter Communication Port (future)
┌────────────────────▼────────────────────────┐
│  LocalSystem or protected service account   │
│  Maankix.Endpoint.Service                   │
│  Worker → Application → Domain              │
└────────────────────┬────────────────────────┘
                     │ Control plane (future)
┌────────────────────▼────────────────────────┐
│  Backend → Knowledge Graph → BI             │
└─────────────────────────────────────────────┘
```

Privilege, code signing, and ELAM / Protected Process Light are installer and driver concerns. They are not configured in this checkpoint.

## 6. Build and quality bar

Every project inherits:

- Nullable reference types
- Implicit usings
- XML documentation file
- Warnings as errors (including CS1591)
- File-scoped namespaces (`.editorconfig`)
- Deterministic builds
- Central package management

Target framework: `net8.0-windows10.0.17763.0` (Windows 10 1809 / Server 2019 and later).

## 7. Evolution rules

1. Do not put product behavior in `Program.cs`.
2. Do not implement a port "just to make DI happy" — register adapters when they exist.
3. Do not add SQLite, HTTP, or graph packages until a checkpoint requires them.
4. When a layering violation is easier than a port, stop and add the port.
5. Record material shifts as new ADRs; do not silently rewrite ADR-001 or ADR-002.
