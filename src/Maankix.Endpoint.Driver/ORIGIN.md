# Provenance

## Upstream

| Field | Value |
| --- | --- |
| Repository | https://github.com/microsoft/Windows-driver-samples |
| Path | `filesys/miniFilter/minispy` |
| Commit | `197ba2156a60e2b76fcd4820bae594223e91a1e9` (2026-08-27) |
| License | Microsoft Public License (MS-PL) — see `MICROSOFT-LICENSE.txt` |
| Learn page | https://learn.microsoft.com/en-us/samples/microsoft/windows-driver-samples/minispy-file-system-minifilter-driver/ |

## Microsoft files (do not rewrite callbacks or kernel comments)

These are MiniSpy sources, kept byte-for-behavior identical except where noted.

| Path | Role |
| --- | --- |
| `filter/minispy.c` | `DriverEntry`, communication port, pre/post callbacks, unload, teardown |
| `filter/RegistrationData.c` | `FLT_OPERATION_REGISTRATION` and `FLT_REGISTRATION` |
| `filter/mspyLib.c` | Log record pool and name helpers |
| `filter/mspyKern.h` | Kernel-only types and prototypes |
| `filter/minispy.rc` | Version resource (still says `minispy.sys` internally) |
| `inc/minispy.h` | Shared records, commands, `MINISPY_PORT_NAME` (`\MiniSpyPort`) |
| `user/mspyUser.c` | MiniSpy console client |
| `user/mspyLog.c` / `user/mspyLog.h` | User-mode log formatting |
| `user/mspyUser.rc` | User-mode version resource |
| `user/minispy.vcxproj` (+ `.Filters`) | User-mode project (output still `minispy.exe`) |
| `README.microsoft.md` | Upstream sample readme |
| `MICROSOFT-LICENSE.txt` | MS-PL |

## Maankix files (rename and documentation only)

| Path | What changed |
| --- | --- |
| `filter/maankixflt.vcxproj` | Renamed from `minispy.vcxproj`; `TargetName` = `maankixflt`; INF path updated |
| `filter/maankixflt.vcxproj.Filters` | INF path updated |
| `maankixflt.inf` | Renamed from `minispy.inf`; service / binary / catalog / provider strings only. Instance names, altitudes (`370000`, `361000`, `385100`), and flags are Microsoft's. |
| `maankixflt.sln` | Renamed from `minispy.sln`; filter project display name `maankixflt` |
| `Directory.Build.props` | Blocks repo-root .NET MSBuild properties; imports WDK NuGet 10.0.26100.6584 |
| `packages.config` / `nuget.config` | WDK + SDK C++ NuGet restore (build only) |
| `msbuild/` | Optional `WindowsKernelModeDriver10.0` toolset shim (not auto-imported) |
| `README.md` / `ORIGIN.md` | This tree's orientation |
| `docs/driver.md`, `docs/BUILD.md`, `docs/DEBUG.md` | Maankix documentation |

## Intentionally unchanged

- Every pre/post callback body and the `Callbacks[]` table
- Communication port name `\MiniSpyPort`
- MiniSpy command IDs (`GetMiniSpyLog`, `GetMiniSpyVersion`)
- Instance altitudes and `Flags = 0x1` (no automatic attach)
- User-mode MiniSpy client protocol

## Not in this tree

No policy engine, no blocking (`FLT_PREOP_COMPLETE` / `STATUS_ACCESS_DENIED`), no Maankix.Endpoint.Service client, no SQLite, no backend, no AI.
