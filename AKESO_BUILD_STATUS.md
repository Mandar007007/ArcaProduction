Environment
  Windows: 11 Enterprise, build 26200
  PowerShell: 5.1
  Admin: NO
  Git: 2.55.0.windows.3
  Python: 3.13.5 host / 3.12 in server image
  Node: v22.22.0 / npm 11.13.0
  Docker: 29.6.2 — RUNNING (user fixed)
  Docker Compose: v5.3.1
  CMake: 4.4.3 installed user-scope via winget Kitware.CMake
  Ninja: 1.13.2 installed user-scope via winget Ninja-build.Ninja
  Visual Studio: 2022 Community 17.14
  WDK: missing
  vcpkg: C:\Users\MandarParekh\vcpkg (baseline/tool mismatch vs agent/vcpkg.json)

Dependencies
  README / DEVELOPMENT.md / Setup-VM.ps1: Python 3.12+, Node 22+, Docker Compose, VS 2022 C++, WDK, CMake 3.20+, Ninja, vcpkg

Repository components
  Official clone: https://github.com/derekxmartin/AkesoDLP.git
  Workspace: c:\xlmProjects\Arca now contains AkesoDLP (Maankix removed)
  server, console, agent, agent/driver, network, proto, docker-compose.yml

Build commands discovered
  docker compose up -d
  make install   # build server+console, up postgres redis server console
  make agent
  cmake -S agent -B agent/build -DCMAKE_BUILD_TYPE=Debug
  cmake --build agent/build --config Debug
  cmake --preset debug / release-driver (docs/DEVELOPMENT.md)
  scripts\Setup-VM.ps1 (Admin)

Run commands discovered
  docker compose up -d postgres redis server console mailhog
  docker compose exec server python -m server.scripts.seed
  Health: http://localhost:8000/api/health
  Console: http://localhost:3000
  Admin: admin / AkesoDLP2026!

Driver installation method
  INF: agent\driver\akeso_dlp_filter.inf
  Service/minifilter: AkesoDLPFilter
  Altitude: 320100
  fltmc load AkesoDLPFilter
  Test signing: bcdedit /set testsigning on (reboot)

Agent installation method
  Foreground: akeso-dlp-agent.exe --console --config config.yaml --test-policy
  Service: akeso-dlp-agent.exe --install ; Start-Service AkesoDLPAgent

Backend startup method
  docker compose build server console
  docker compose up -d postgres redis
  docker compose up -d server
  docker compose exec server python -m server.scripts.seed

Console startup method
  docker compose up -d console
  (or cd console && npm install && npm run dev)

Current blockers
  Driver: no WDK, not Admin
  Agent: not built
  Full compose (http-proxy/smtp-relay) fails lxml build
  Host 5432 occupied — Akeso postgres mapped to 5433
