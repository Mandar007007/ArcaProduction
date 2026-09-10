# Build maankixflt

Build the minifilter with the Windows Driver Kit. Do **not** use `dotnet build` on `maankixflt.sln`. `dotnet build Maankix.sln` remains the .NET service only.

## Prerequisites

| Component | Version used in this repo |
| --- | --- |
| Windows | 10 1809+ / 11 / Server 2019+ (x64) |
| Visual Studio | 2022 17.14 with Desktop development with C++ |
| Windows SDK | 10.0.26100 |
| Windows Driver Kit | 10.0.26100 (winget id `Microsoft.WindowsWDK.10.0.26100`) |
| WDK Visual Studio extension | Installed by the WDK setup for VS 2022 |

Two ways to get the WDK (pick one):

**A. WDK NuGet (preferred for this repo / CI / locked-down machines)**

```powershell
cd C:\xlmProjects\Arca\src\Maankix.Endpoint.Driver
Invoke-WebRequest https://dist.nuget.org/win-x86-commandline/latest/nuget.exe -OutFile $env:TEMP\nuget.exe
& $env:TEMP\nuget.exe restore packages.config -PackagesDirectory packages -ConfigFile nuget.config
```

`Directory.Build.props` imports `Microsoft.Windows.WDK.x64` **10.0.26100.6584** (and the matching SDK C++ packages). The `packages\` folder is gitignored.

**B. Full WDK MSI** (if group policy allows)

```powershell
winget install --id Microsoft.WindowsWDK.10.0.26100 --accept-package-agreements --accept-source-agreements
```

Then confirm:

```powershell
Test-Path "${env:ProgramFiles(x86)}\Windows Kits\10\Include\10.0.26100.0\km\fltKernel.h"
```

The WDK MSI may fail with `0x800704EC` (blocked by group policy). Use path A for headers and libs.

You still need the **WDK Visual Studio extension** so MSBuild can resolve `WindowsKernelModeDriver10.0`. That ships with the WDK setup and writes toolsets under the VS `PlatformToolsets` folder. Without it, MSBuild reports MSB8020 even after a successful NuGet restore. Re-run WDK setup (or the VS installer “Windows Driver Kit” component) on a machine that allows it, then rebuild.

A local toolset shim lives in `src/Maankix.Endpoint.Driver/msbuild/` for reference. Do not set `AdditionalVCTargetsPath` to that folder; it hijacks `VCTargetsPath` and breaks MSVC imports.

## Build (x64 Debug)

From an **x64 Native Tools** or **Developer Command Prompt for VS 2022**:

```powershell
cd C:\xlmProjects\Arca\src\Maankix.Endpoint.Driver

& "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" `
  maankixflt.sln `
  /p:Configuration=Debug `
  /p:Platform=x64 `
  /m
```

Release:

```powershell
& "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" `
  maankixflt.sln `
  /p:Configuration=Release `
  /p:Platform=x64 `
  /m
```

Outputs (paths vary slightly with WDK packaging):

| Artifact | Typical location |
| --- | --- |
| `maankixflt.sys` | `filter\x64\Debug\` or `filter\x64\Debug\maankixflt\` |
| `maankixflt.pdb` | next to the `.sys` |
| `maankixflt.inf` | stamped copy next to the `.sys` |
| `minispy.exe` | `user\x64\Debug\` |

ARM64 configurations exist in the Microsoft project files; this checkpoint documents x64 only.

## Test-sign and load (elevated)

An unsigned or test-signed minifilter will not load on a production-signed Windows boot. For a development machine:

```powershell
bcdedit /set testsigning on
# reboot
```

Install and start (names match `maankixflt.inf`):

```powershell
# Run from the directory that contains the built maankixflt.sys and stamped INF
pnputil /add-driver maankixflt.inf /install

sc.exe query maankixflt
sc.exe start maankixflt
fltmc.exe filters
```

Demand-start (`StartType = 3`). Filter Manager also accepts:

```powershell
fltmc.exe load maankixflt
fltmc.exe unload maankixflt
sc.exe stop maankixflt
```

Instances do **not** auto-attach (`Flags = 0x1`). Attach is optional and is still the MiniSpy user client or:

```powershell
fltmc.exe attach maankixflt C:
fltmc.exe detach maankixflt C:
```

This checkpoint's success criteria are: the image compiles, the service starts, `fltmc filters` shows `maankixflt`, and unload succeeds. Attaching and tracing I/O is Microsoft MiniSpy behavior, not Maankix product work.

## What not to wire up

- Do not point `Maankix.Endpoint.Service` at `\MiniSpyPort`.
- Do not change altitudes without a Microsoft-assigned allocation.
- Do not add a deny path in `SpyPreOperationCallback`.
