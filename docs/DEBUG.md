# Debug maankixflt with WinDbg

Symbols and source live with the WDK build. The .NET service is a separate process and is not involved.

## Artifacts

After a Debug x64 build:

| File | Use |
| --- | --- |
| `maankixflt.sys` | Loaded image |
| `maankixflt.pdb` | Private symbols (keep next to the `.sys` or on a symbol path) |
| `filter\minispy.c` | `DriverEntry`, port, pre/post, unload |
| `filter\RegistrationData.c` | `Callbacks[]` |

Microsoft public symbols are not required for this sample if you use the PDB you just built.

## Target machine

1. Enable test signing (`bcdedit /set testsigning on`) and reboot.
2. Install / start `maankixflt` as in [BUILD.md](BUILD.md).
3. For local kernel debugging (Windows 10+), enable it and reboot:

```powershell
bcdedit /debug on
bcdedit /dbgsettings local
```

Two-machine KDNET is more reliable for breaking in `DriverEntry` (the filter may already be running if you attach after load):

```powershell
# target
bcdedit /debug on
bcdedit /dbgsettings net hostip:192.0.2.10 port:50000 key:1.2.3.4
```

Install [WinDbg](https://learn.microsoft.com/windows-hardware/drivers/debugger/) (Windows SDK / Microsoft Store) on the host.

## Symbol path

```
.sympath srv*C:\symbols*https://msdl.microsoft.com/download/symbols
.sympath+ C:\xlmProjects\Arca\src\Maankix.Endpoint.Driver\filter\x64\Debug
.srcpath C:\xlmProjects\Arca\src\Maankix.Endpoint.Driver
.reload /f maankixflt.sys
lm m maankixflt
```

If the module name in `lm` is `maankixflt`, use that name in breakpoints. If FltMgr loaded it under another name, `lm m *flt*` and use the exact module.

## Breakpoints

Load-time (set these **before** `sc start` / `fltmc load`, or use a boot breakpoint):

```
bu maankixflt!DriverEntry
bu maankixflt!SpyFilterUnload
```

I/O path (very hot — expect flooding after attach):

```
bu maankixflt!SpyPreOperationCallback
bu maankixflt!SpyPostOperationCallback
```

Port:

```
bu maankixflt!SpyConnect
bu maankixflt!SpyDisconnect
bu maankixflt!SpyMessage
```

Conditional example (creates only; requires symbols and a known `Data` layout — use as a starting point, not a product hook):

```
bp maankixflt!SpyPreOperationCallback
```

Then in the debugger inspect `Data->Iopb->MajorFunction` and continue unless it is `IRP_MJ_CREATE` (`0x00`).

## Filter Manager extensions

```
!fltkd.filters
!fltkd.instances
!fltkd.portlist
```

Confirm `maankixflt` is registered and whether any instance is attached. With INF flags `0x1`, a successful load can show **zero** instances until `fltmc attach` or `minispy.exe`.

## Typical sessions

**Compile / load / unload**

1. `bu maankixflt!DriverEntry` then start the service — hit registration and `FltCreateCommunicationPort`.
2. `!fltkd.filters` — filter present.
3. `bu maankixflt!SpyFilterUnload` then `fltmc unload maankixflt` — port close, unregister, lookaside delete.

**Why you see no I/O**

The filter is loaded but not attached. That is Microsoft MiniSpy's default. Do not treat "no pre-callback hits" as a failed load.

## Notes

- `SpyPreOperationCallback` / `SpyPostOperationCallback` are non-paged. Do not call paged APIs from a debugger expression that you then paste into the driver.
- Breaking on every pre-callback on an attached volume will stall the machine. Prefer `DriverEntry` / unload until you need I/O.
- The Maankix Worker (`Maankix.Endpoint.Service`) has no kernel breakpoints.
