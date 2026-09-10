# Maankix

Windows Endpoint Data Loss Prevention (DLP) platform.

This repository is the production foundation for the Maankix endpoint stack. The first checkpoint delivers the host, layering, build system, and documentation that a long-lived engineering team can extend. It does not implement product behavior.

## Platform architecture

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

This repository currently ships the **Windows Endpoint Service** host and the **maankixflt** minifilter (vendored Microsoft MiniSpy). The service does not open the filter port. Policies, backend APIs, persistence, REST, the knowledge graph, and behavior intelligence are out of scope.

## Repository layout

```
.
├── Directory.Build.props          Shared compiler and assembly metadata
├── Directory.Packages.props       Central package versions
├── global.json                    .NET 8 SDK pin
├── Maankix.sln
├── docs/
│   ├── architecture.md
│   ├── driver.md
│   ├── BUILD.md
│   ├── DEBUG.md
│   └── adr/
│       ├── ADR-001-lightweight-clean-architecture.md
│       └── ADR-002-dotnet8-worker-windows-service.md
└── src/
    ├── Maankix.Endpoint.Service/  .NET 8 Worker Windows Service
    └── Maankix.Endpoint.Driver/   maankixflt (MiniSpy) — WDK, not dotnet
```

## Endpoint service layers

| Layer | Responsibility | May reference |
| --- | --- | --- |
| Domain | Ports and enterprise concepts | Nothing in this repository |
| Application | Use-case composition contracts | Domain |
| Infrastructure | Adapters and OS-facing registration | Domain |
| Worker | Hosted process lifetime | Domain, Application, Configuration |
| Configuration | Composition helpers, options, logging | All layers |
| Common | Shared constants | Nothing layer-specific |

Domain must never reference Infrastructure. `Program.cs` is the composition root and contains no business logic.

## Build

Prerequisites: Windows 10 version 1809 or later, [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (see `global.json`).

```powershell
dotnet restore Maankix.sln
dotnet build Maankix.sln --configuration Release
```

Warnings are errors. Public API requires XML documentation. Nullable reference types are enabled.

## Run (interactive)

```powershell
dotnet run --project src/Maankix.Endpoint.Service --configuration Debug
```

`DOTNET_ENVIRONMENT=Development` is set by `Properties/launchSettings.json`. Console logging uses the simple formatter in Development and JSON (structured) otherwise.

## Install as a Windows Service

Build Release, then register the binary with Service Control Manager. Run an elevated prompt:

```powershell
dotnet publish src/Maankix.Endpoint.Service -c Release -o C:\Maankix\Endpoint

sc.exe create MaankixEndpoint binPath= "C:\Maankix\Endpoint\Maankix.Endpoint.Service.exe" start= auto
sc.exe description MaankixEndpoint "Maankix Windows endpoint data-loss prevention host."
sc.exe start MaankixEndpoint
```

The process uses `AppContext.BaseDirectory` as the content root when SCM launches it, so `appsettings.json` is read from the publish directory rather than `C:\Windows\System32`.

Event Log source `Maankix Endpoint Service` is written to the Application log. Creating a custom source the first time may require elevation; later checkpoints should register the source during install.

## Configuration

`src/Maankix.Endpoint.Service/appsettings.json`

| Key | Meaning |
| --- | --- |
| `Maankix:Endpoint:ServiceName` | Logical name used in structured logs |
| `Maankix:Endpoint:ShutdownTimeout` | Generic-host stop timeout |
| `Logging` | Log levels, console formatter, Event Log |

User secrets id: `maankix-endpoint-service-0001`.

## Design principles

SOLID, DRY, KISS, YAGNI, Clean Architecture, Clean Code, and Microsoft .NET Design Guidelines. Extensibility is preserved through empty ports (`IEndpointIdentity`, `IFilterCommunicationPort`, `IEndpointModule`) without speculative implementations.

## Minifilter

`src/Maankix.Endpoint.Driver` is Microsoft MiniSpy, renamed to **maankixflt**. Callbacks are untouched. Build with MSBuild + WDK:

- [Driver architecture](docs/driver.md)
- [BUILD.md](docs/BUILD.md)
- [DEBUG.md](docs/DEBUG.md)
- [ORIGIN.md](src/Maankix.Endpoint.Driver/ORIGIN.md)

## Documentation

- [Architecture](docs/architecture.md)
- [ADR-001 Lightweight Clean Architecture](docs/adr/ADR-001-lightweight-clean-architecture.md)
- [ADR-002 .NET 8 Worker Windows Service](docs/adr/ADR-002-dotnet8-worker-windows-service.md)

## Out of scope (this checkpoint)

Service-to-driver communication, policies, backend, SQLite, REST, knowledge graph, and AI / behavior intelligence.
