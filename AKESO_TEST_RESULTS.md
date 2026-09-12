# AkesoDLP Local Test Results

## Environment

Windows: 11 Enterprise, build 26200
Visual Studio: 2022 Community 17.14 (MSVC present)
WDK: MISSING (no km\fltKernel.h; WDK MSI previously GPO-blocked)
Python: 3.13.5 (host); server runs in Docker python:3.12
Node: v22.22.0
Docker: 29.6.2 / Compose v5.3.1 — RUNNING
CMake: 4.4.3 (user-scope winget, not on default PATH)

## Backend

Status: PASS
URL: http://localhost:8000
Health check: HTTP 200 {"status":"ok","service":"akeso-dlp-server"}
gRPC: listening on :50051 (server log: "gRPC server started")
Required services: postgres (healthy, host port 5433), redis (healthy), server (healthy), mailhog (healthy)
Login: PASS (POST /api/auth/login admin / AkesoDLP2026!)
Seed: PASS (roles, admin, 10 identifiers, 6 templates)

## Console

Status: PASS
URL: http://localhost:3000
HTTP: 200

## Driver

Driver: NOT BUILT
SYS path: none (Get-ChildItem *.sys returned empty)
Service: AkesoDLPFilter (not installed)
Minifilter: AkesoDLPFilter
Loaded: NO
Attached: NO

## Endpoint Agent

Executable: NOT BUILT (no akeso-dlp-agent.exe)
Status: not running
Backend connected: NO
Driver connected: NO

## Endpoint Registration

Status: FAIL — 0 agents registered (GET /api/agents total=0)

## Tests

### File → USB

Result: NOT RUN
Evidence: Kernel driver .sys does not exist; cannot intercept USB writes. Server-side file detect on C:\AkesoDLP-Test\test-sensitive.txt returned match_count=2 (US SSN 123-45-6789, Email jane.test@example.com).

### Clipboard

Result: NOT RUN
Evidence: Requires running akeso-dlp-agent.exe --console --test-policy. Agent binary not built.

### Browser Upload

Result: NOT RUN
Evidence: Agent browser monitor + optional http-proxy image. http-proxy/smtp-relay Docker builds failed (lxml needs libxml2/libxslt in those Dockerfiles). Core stack started without those services.

### Network Share

Result: NOT RUN
Evidence: Same as USB — minifilter not loaded.

### Endpoint Discover

Result: NOT RUN
Evidence: Discover is agent-driven; no agent online. Test files exist at C:\AkesoDLP-Test\.

### Server-side detection (extra, works without agent)

Result: PASS
Evidence: POST /api/detect on dummy PCI/PII text → US SSN match. POST /api/detect/file on test-sensitive.txt → SSN + email. Policy "Local Test PCI Block" created from pci_dss template and activated (id 7fcbc1d6-b898-4bc9-b353-794e9ce1564d, status=active).

## Blockers

1. Not running as Administrator — cannot load minifilter / run Setup-VM.ps1 / enable test signing.
2. WDK kernel headers missing — cannot produce akeso_dlp_filter.sys on this machine.
3. Agent not built — vcpkg baseline/tool mismatch; Makefile CMake path also needs CMake on PATH inside VS Dev Shell.
4. docker compose up -d of ALL services fails on http-proxy and smtp-relay image build (lxml). Core services started via documented make install path (server + console + postgres + redis).
5. Host port 5432 already used by manava-ui-backend-postgres-1. Akeso postgres published as 5433:5432 (containers still use postgres:5432 internally).

## Files Modified

- docker-compose.yml — postgres host port 5432 → 5433 to avoid bind conflict
- Workspace replaced Maankix tree with AkesoDLP sources (prior session)
- AKESO_BUILD_STATUS.md — recorded discovered commands
- AKESO_TEST_RESULTS.md — this file
- C:\AkesoDLP-Test\ — dummy test files only (not in repo)
