# ADR-002: Host the endpoint as a .NET 8 Worker Windows Service

- Status: Accepted
- Date: 2026-09-10
- Deciders: Maankix endpoint platform

## Context

The user-mode side of Maankix must start at boot, survive logoff, restart on failure, and later host filter and policy work. Classic options:

1. Custom `ServiceBase` / `ServiceInstaller` wrapper around a console exe
2. A Windows Service written in C++/ATL that hosts CLR
3. .NET Generic Host Worker (`Microsoft.NET.Sdk.Worker`) with `AddWindowsService`
4. IIS / ASP.NET host (rejected: this is not a web server)

The service must also support interactive runs for developers (`dotnet run`) without a second codebase.

We require .NET 8, nullable reference types, generic-host dependency injection, `IConfiguration`, and structured logging from day one.

## Decision

Implement the endpoint as a **.NET 8 Worker Service** targeting `net8.0-windows10.0.17763.0`:

- SDK: `Microsoft.NET.Sdk.Worker`
- Host: `Host.CreateApplicationBuilder`
- SCM lifetime: `Microsoft.Extensions.Hosting.WindowsServices` (`AddWindowsService`)
- Process identity constants: `ServiceIdentity` (`MaankixEndpoint`)
- Content root: `AppContext.BaseDirectory` when `WindowsServiceHelpers.IsWindowsService()` is true, so `appsettings.json` is not resolved from `C:\Windows\System32`
- Worker: `EndpointWorker` : `BackgroundService` — idle wait on the stopping token
- Logging: JSON console + Event Log + Debug, levels from `appsettings.json`
- Options: `EndpointServiceOptions` bound from `Maankix:Endpoint`, validated at start

`Program.cs` only creates the builder, calls `ConfigureMaankixEndpoint()`, and runs the host.

The SDK is pinned in `global.json` to the 8.0 feature band so a machine-wide .NET 9 SDK does not silently retarget the product.

## Consequences

Positive:

- One process model for development and production.
- DI, configuration, logging, and graceful shutdown are framework features, not invented here.
- `UseWindowsService` / `AddWindowsService` integrates with SCM stop/start without a custom `ServiceBase`.
- Structured logs are available before any product feature exists.

Negative:

- First Event Log write with a custom source may require elevation or an installer step. Accepted; install is a later checkpoint.
- `net8.0-windows10.0.17763.0` excludes non-Windows CI agents unless they enable Windows targeting packs. Accepted; this binary is Windows-only.
- Generic Host is not a real-time I/O loop. Filter-port threads, if added later, must not run on the hosted-service delay path.

Follow-up (not in this checkpoint):

- Service installer (MSI / WiX or equivalent) that creates the SCM entry, recovery actions, and Event Log source.
- Failure recovery (`sc failure`) and delayed auto-start policy.
- Process protection / service account hardening.
