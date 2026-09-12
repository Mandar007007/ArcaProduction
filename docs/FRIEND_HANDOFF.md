# AkesoDLP — Friend handoff (from zero to “I saw it block a file”)

This is for the person with a **Windows PC and Administrator access**.
Read it like a recipe. Do the steps in order. Do not skip the “what this means” bits if you have never run a DLP stack before.

**Login (after the website is up)**

- Open: http://localhost:3000
- Username: `admin`
- Password: `AkesoDLP2026!`

If the page says the API is not running, the website container cannot talk to the server. That is already fixed in this repo (`DLP_API_PROXY=http://server:8000`). Recreate the console container (Step 4).

---

## 0. What you are building (the toy-town version)

Imagine a school hallway.

| Piece | Kid version | Real name |
| --- | --- | --- |
| **Guard at the door** | Stops a backpack *before* it leaves the building | Kernel **minifilter driver** (`akeso_dlp_filter.sys`, service `AkesoDLPFilter`) |
| **Teacher with the rulebook** | Reads the backpack, applies rules, says ALLOW or BLOCK | **Endpoint agent** (`akeso-dlp-agent.exe`) |
| **Office** | Stores rules, users, incidents | **Server** (Python, port **8000**, gRPC **50051**) |
| **Whiteboard** | Website you click | **Console** (React, port **3000**) |
| **Filing cabinet** | Remembers users and policies after restart | **PostgreSQL** (Docker; published on host port **5433**) |
| **Sticky-note board** | Fast temporary messages | **Redis** (port **6379**) |

**End-to-end path you want to see**

```
You copy test-sensitive.txt to a USB stick
        ↓
Windows asks the driver: “may this write happen?”
        ↓
Driver asks the agent (port \AkesoDLPPort)
        ↓
Agent scans dummy credit-card / SSN text
        ↓
Policy says BLOCK
        ↓
Windows shows Access Denied
        ↓
Agent reports an incident to the server
        ↓
You see it on http://localhost:3000 → Incidents
```

Without the driver, USB/network-share **block** cannot happen.
Without the agent, the driver has nobody to ask.
Without Docker server+console, you can still block locally with `--test-policy`, but you will not see the endpoint online in the website.

This is a **test / education** DLP. Do not put it on a work laptop that is not yours to modify. Test signing weakens Windows driver trust **on that machine** until you turn it off.

---

## 1. What your machine must already have

Do this **before** git pull if you can.

### Must have (website + API)

1. **Windows 10/11** (64-bit).
2. **Docker Desktop** installed and **actually running** (whale icon in the tray, not stuck on “Starting”).
3. **Git**.

### Must have (driver + agent — this is the hard part)

4. **Administrator** rights on this PC.
5. **Visual Studio 2022** with the **Desktop development with C++** workload.
6. **Windows SDK** (usually comes with VS).
7. **Windows Driver Kit (WDK)** that matches the SDK (kernel headers).  
   Check in PowerShell:

   ```powershell
   Test-Path "${env:ProgramFiles(x86)}\Windows Kits\10\Include\10.0.26100.0\km\fltKernel.h"
   ```

   If that is `False`, install the WDK from Microsoft **as Administrator**. Company laptops often **block** the WDK installer. If that happens, you cannot load a homemade `.sys` the normal way.
8. **CMake 3.20+** (`cmake --version`).
9. Optional but useful: **Ninja**, **vcpkg** (agent libraries: gRPC, Hyperscan, …).

### Ports that must be free (or you change them)

| Port | Who uses it |
| --- | --- |
| 3000 | Console website |
| 8000 | Server HTTP API |
| 50051 | Agent ↔ server gRPC |
| 5433 | Postgres on the **host** (inside Docker it is still 5432) |
| 6379 | Redis |

If 5432 is already taken by another app, that is OK. This repo maps Postgres to **5433** on purpose.

Do **not** run `docker compose up -d` for **every** service. The HTTP-proxy and SMTP-relay images currently fail to build (`lxml` / libxml2). Start only the core four (Step 4).

---

## 2. Pull the code

```powershell
git clone <THE_REPO_URL> C:\AkesoDLP
cd C:\AkesoDLP
```

Or if you already cloned:

```powershell
cd C:\AkesoDLP
git pull
```

You should see folders: `server`, `console`, `agent`, `docker-compose.yml`, `scripts`.

---

## 3. Start Docker Desktop

1. Open **Docker Desktop**.
2. Wait until it says it is running (not “Starting…”).
3. Test:

   ```powershell
   docker version
   docker compose version
   ```

   You must see a **Server** version, not only Client. If `docker ps` hangs, Docker is not ready.

---

## 4. Start the office (server + website)

In the repo root (`C:\AkesoDLP`):

```powershell
docker compose build server console
docker compose up -d postgres redis
docker compose up -d server
docker compose up -d console mailhog
docker compose ps
```

Wait until **postgres, redis, server, console** say `healthy` (or at least `Up`).

**What this did**

- Downloaded/built Linux containers.
- Started a database, a cache, the API, and the website.
- The website inside Docker talks to the API as `http://server:8000` (Docker DNS name). Your browser still uses http://localhost:3000.

**Prove the office is alive**

```powershell
(Invoke-WebRequest http://localhost:8000/api/health).Content
(Invoke-WebRequest http://localhost:3000).StatusCode
(Invoke-WebRequest http://localhost:3000/api/health).Content
```

You want:

- API: `{"status":"ok","service":"akeso-dlp-server"}`
- Console page: `200`
- Console **proxy** health (third line): same JSON as the API

If the third line fails but the first works, recreate console:

```powershell
docker compose up -d --force-recreate console
```

---

## 5. Create the admin user and dummy policies

The empty database has tables but no login until you seed:

```powershell
docker compose exec server python -m server.scripts.seed
```

You should see: admin user, 10 data identifiers, 6 policy templates.

Optional demo data (lots of fake incidents):

```powershell
docker compose exec server python -m server.scripts.demo_seed
```

---

## 6. Log in and click around (no driver needed)

1. Browser: **http://localhost:3000**
2. `admin` / `AkesoDLP2026!`
3. MFA is off for this admin. You should land on the dashboard.

**What you can test immediately (server-only)**

1. **Policies** — templates exist (PCI-DSS, HIPAA, …). Most start **suspended**.
2. Create a real policy from a template (API or UI):
   - Template name: `pci_dss`
   - Then **Activate** it.
3. Dummy files (create them):

   ```powershell
   New-Item -ItemType Directory -Path C:\AkesoDLP-Test -Force | Out-Null
   Set-Content C:\AkesoDLP-Test\normal.txt "This is a normal document with no sensitive content."
   Set-Content C:\AkesoDLP-Test\test-sensitive.txt @"
   CONFIDENTIAL Report
   Customer: Jane Test
   SSN: 123-45-6789
   Credit Card: 4111111111111111
   Email: jane.test@example.com
   INTERNAL ONLY
   "@
   ```

   Use **4111111111111111** (no dashes). Dashed `4111-1111-1111-1111` often does **not** match the PCI pattern.

4. Scan via API docs: http://localhost:8000/docs  
   `POST /api/detect` with that text, or `POST /api/detect/file` with `test-sensitive.txt`.  
   Expect matches for **SSN** and **email** (and card if the digits are continuous).

**Agents page will show 0 online** until Step 10. That is normal.

---

## 7. Build the kernel driver (needs WDK + VS)

The `.sys` file is a **build output**. It is usually **not** in git (`agent/build/` is ignored; do not assume a `.sys` arrived from `git pull`).

**Option A — repo script (if you already built once on this machine)**

Skip to Step 8 if this file exists:

`agent\build-driver\driver\Release\akeso_dlp_filter.sys`

**Option B — official CMake preset (needs WDK installed in Program Files)**

Open **x64 Native Tools Command Prompt for VS 2022** (or Dev PowerShell):

```powershell
cd C:\AkesoDLP\agent
cmake --preset release-driver
cmake --build build/release-driver --target akeso_dlp_filter
```

**Option C — same sources, Visual Studio generator**

```powershell
cd C:\AkesoDLP
cmake -S agent -B agent/build-driver -G "Visual Studio 17 2022" -A x64 -DBUILD_DRIVER=ON -DBUILD_TESTS=OFF
cmake --build agent/build-driver --config Release --target akeso_dlp_filter
```

Success = a file named **`akeso_dlp_filter.sys`**. If there is no `.sys`, **stop**. Do not install air.

Copy it next to the INF (the installer looks here too):

```powershell
Copy-Item C:\AkesoDLP\agent\build-driver\driver\Release\akeso_dlp_filter.sys C:\AkesoDLP\agent\driver\akeso_dlp_filter.sys -Force
```

---

## 8. Install and load the driver (Administrator — required)

Windows will not let a normal user load a kernel driver. That is the lock on the school door.

1. Close everything you care about. You may need a **reboot**.
2. Start menu → type **PowerShell** → right-click → **Run as administrator**.
3. Run:

   ```powershell
   cd C:\AkesoDLP
   powershell -ExecutionPolicy Bypass -File .\scripts\Install-AkesoDriver.ps1
   ```

**What that script does (in English)**

1. Turns on **test signing** (`bcdedit /set testsigning on`).  
   Windows will then accept a **test certificate** we created, not a Microsoft-bought EV cert.  
   First time: **reboot**, then run the script **again**.
2. Creates a code-signing cert named `AkesoDLP Test` and trusts it.
3. Signs the `.sys`.
4. Copies it to `C:\Windows\System32\drivers\akeso_dlp_filter.sys`.
5. Installs `agent\driver\akeso_dlp_filter.inf` (service name **AkesoDLPFilter**, altitude **320100**).
6. `fltmc load AkesoDLPFilter`.

**Prove it loaded**

```powershell
sc.exe query AkesoDLPFilter
fltmc
fltmc instances
```

You want:

- Service exists (not error 1060).
- `fltmc` lists **AkesoDLPFilter**.
- `fltmc instances` shows it attached to volumes (USB / shares when present).

If `fltmc load` fails after the first run: **reboot** (test signing), then run the script again.

To unload later:

```powershell
fltmc unload AkesoDLPFilter
```

To turn test signing off when you are done (reboot again):

```powershell
bcdedit /set testsigning off
```

---

## 9. Build and start the agent

The agent is a Windows program. It talks to:

- the driver via `\AkesoDLPPort`
- the server via gRPC on **localhost:50051**

**Build (documented)**

From VS x64 Native Tools, with `VCPKG_ROOT` set if you use vcpkg:

```powershell
cd C:\AkesoDLP\agent
cmake --preset debug
cmake --build build/debug --target akeso-dlp-agent
```

Or Makefile-style:

```powershell
cmake -S C:\AkesoDLP\agent -B C:\AkesoDLP\agent\build -DCMAKE_BUILD_TYPE=Debug
cmake --build C:\AkesoDLP\agent\build --config Debug --target akeso-dlp-agent
```

Find the exe (do not guess the name):

```powershell
Get-ChildItem C:\AkesoDLP\agent -Recurse -Filter akeso-dlp-agent.exe
```

**Config**

```powershell
Copy-Item C:\AkesoDLP\agent\config\config.yaml.example C:\AkesoDLP\agent\config\config.yaml
```

Edit `server.host` to `localhost` and `server.port` to `50051`. For a first USB test you can skip TLS (`tls.enabled: false` if the example has it on — the Docker gRPC server is started without TLS in the current compose `DLP_DEBUG=true` setup). If the agent refuses to connect, check `agent` logs and `docker compose logs server`.

**Run in a console (easiest to see logs)** — **Administrator** recommended (driver port):

```powershell
cd C:\AkesoDLP\agent\build\debug
.\akeso-dlp-agent.exe --console --config C:\AkesoDLP\agent\config\config.yaml --test-policy
```

`--test-policy` loads built-in demo policies (PCI / PII / confidential keywords) so USB block works even before policy sync.

You want log lines like: driver connected, clipboard started, gRPC connected / registered.

**Optional Windows service**

```powershell
.\akeso-dlp-agent.exe --install
Start-Service AkesoDLPAgent
```

---

## 10. See the endpoint in the website

1. Keep Docker server up.
2. Keep the agent running.
3. Refresh **Agents** on http://localhost:3000.

The agent should appear **online** after it registers over gRPC.

If Agents stays empty:

- `docker compose logs server` — look for `gRPC server started on port 50051`
- From the PC: is anything listening on 50051?
- Agent log: connection error (TLS mismatch is the usual one)

---

## 11. End-to-end tests (dummy data only)

Use **fake** SSN/card numbers only. Never real employee data.

### Test A — USB block (the main demo)

1. Driver loaded (`fltmc`).
2. Agent running with `--test-policy`.
3. Plug in a USB stick (note the drive letter, e.g. `E:`).
4. Copy:

   ```powershell
   copy C:\AkesoDLP-Test\test-sensitive.txt E:\
   ```

5. **Pass:** copy fails (Access Denied). Agent log shows SCAN / VIOLATION / BLOCK. Recovery folder may get a copy (`C:\AkesoDLP\Recovery`). Website **Incidents** gets a new row (if gRPC reporting works).
6. Control: `copy C:\AkesoDLP-Test\normal.txt E:\` should **succeed**.

### Test B — Clipboard

1. Agent running.
2. Open `test-sensitive.txt` in Notepad, select all, Ctrl+C.
3. Agent log should show CLIPBOARD / violation.
4. This is user-mode. It does **not** need USB.

### Test C — Browser upload

1. Agent running (ETW monitor).
2. Attach `test-sensitive.txt` in a **local** test form if you have one. Do not upload real secrets to the internet.
3. Akeso’s own notes: ETW detection can be **after** the send. True pre-block needs more than this PoC hook.

### Test D — Network share

Same as USB, but copy to `\\localhost\SomeShare`. The driver only cares that the volume is a network volume.

### Test E — Endpoint Discover

Point discover at `C:\AkesoDLP-Test` in agent config (or console Discover job if the UI creates one), run the scan, check console Discover results.

---

## 12. Daily start / stop (after the first setup)

**Start (normal user is enough for Docker):**

```powershell
cd C:\AkesoDLP
docker compose up -d postgres redis server console mailhog
```

**Start driver (Admin):**

```powershell
fltmc load AkesoDLPFilter
```

**Start agent (Admin if talking to the driver):**

```powershell
.\akeso-dlp-agent.exe --console --config ... --test-policy
```

**Stop:**

```powershell
# Agent: Ctrl+C
fltmc unload AkesoDLPFilter
docker compose stop
```

**Wipe Docker data (destroys the database):**

```powershell
docker compose down -v
```

Then do Step 4 and Step 5 again.

---

## 13. If something breaks

| Symptom | Likely cause |
| --- | --- |
| Login: “API server is not running” | Console proxy not pointing at `http://server:8000`. Recreate console. |
| `docker compose up` fails on http-proxy / smtp | Ignore those services. Start only postgres/redis/server/console. |
| Port 5432 in use | This repo uses **5433** on the host. Do not fight the other Postgres. |
| No `.sys` | WDK missing or CMake skipped the driver. |
| `fltmc` Access denied | You are not Administrator. |
| `fltmc load` fails after first install | Reboot for test signing, run installer again. |
| USB copy still works for secrets | Driver not attached to that volume, or agent not connected to `\AkesoDLPPort`, or `--test-policy` / active policy missing. |
| Agents = 0 | Agent not running, or gRPC/TLS mismatch, or server gRPC failed at startup. |
| WDK installer blocked (0x800704EC) | Company GPO. Need a machine that allows WDK, or an IT exception. |

---

## 14. What Mandar’s laptop already proved

- Docker **server + console + seed + login** work.
- Console proxy bug is fixed in `console/vite.config.ts` + compose `DLP_API_PROXY`.
- A `.sys` **was compiled** on Mandar’s PC using WDK NuGet headers (file is a local build; **rebuild on your PC**).
- Mandar **cannot** `fltmc load` (no Admin; GPO blocks UAC-from-script). That is why you are doing Step 8.

When USB block works, you are done: driver → agent → policy → BLOCK → incident.
