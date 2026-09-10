# maankixflt

Windows file-system minifilter for Maankix. This folder is Microsoft's official **MiniSpy** sample, vendored and renamed so the binary and SCM name are `maankixflt`. Callbacks, comments, altitudes, and port protocol are unchanged.

This checkpoint is **compile / load / unload / understand** only. The .NET endpoint service does not connect to this driver.

| Document | Purpose |
| --- | --- |
| [docs/driver.md](../../docs/driver.md) | Folder map, DriverEntry, registration, port, callbacks, lifecycle |
| [docs/BUILD.md](../../docs/BUILD.md) | Exact WDK build and load steps |
| [docs/DEBUG.md](../../docs/DEBUG.md) | WinDbg |
| [ORIGIN.md](ORIGIN.md) | Microsoft files vs Maankix files |
| [README.microsoft.md](README.microsoft.md) | Upstream MiniSpy readme |
| [MICROSOFT-LICENSE.txt](MICROSOFT-LICENSE.txt) | MS-PL from Windows-driver-samples |

Build the driver from `maankixflt.sln` with MSBuild + WDK, not `dotnet build`.
