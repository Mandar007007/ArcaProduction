# ADR-001: Lightweight Clean Architecture for the endpoint service

- Status: Accepted
- Date: 2026-09-10
- Deciders: Maankix endpoint platform

## Context

Maankix will grow from a process host into a multi-year endpoint security product. The service will eventually own filter I/O, policy evaluation, local persistence, and control-plane sync. Those concerns must not collapse into a single "god worker."

A full multi-project Clean Architecture (separate Domain / Application / Infrastructure assemblies plus an API host) is the usual enterprise default. At this checkpoint there is one deployable, zero use cases, and a requirement to stay maintainable without overengineering.

We needed a structure that:

- Makes the dependency rule obvious to every engineer
- Keeps Domain free of Infrastructure
- Leaves `Program.cs` as a composition root
- Can split into assemblies later without renaming types

## Decision

Organize `Maankix.Endpoint.Service` as a **single Worker project** with folders that match the layers:

- `Domain/` — enterprise ports only
- `Application/` — use-case contracts only
- `Infrastructure/` — adapters and OS-facing registration
- `Worker/` — `IHostedService` process lifetime
- `Configuration/` — host composition, options, logging
- `Common/` — cross-cutting constants that are not domain rules

Layer reference rules:

1. Domain must not reference Infrastructure (or Worker, or Configuration).
2. Application may reference Domain.
3. Infrastructure may reference Domain.
4. Domain and Application must not take a dependency on `Microsoft.Extensions.DependencyInjection`. Their future services are registered from `Configuration`.

Ports introduced now (`IEndpointIdentity`, `IFilterCommunicationPort`, `IEndpointModule`) stay empty. A method on a port is a product decision, not a scaffolding exercise.

Enforcement in this checkpoint is convention plus review, documented in `docs/architecture.md`. Automated architecture tests can be added when a test project exists.

## Consequences

Positive:

- New engineers can find the correct folder without a solution-folder maze.
- Domain stays a pure C# surface; no host types leak inward.
- A later split to `Maankix.Endpoint.Domain` (and siblings) is a project-file change, not a redesign.
- YAGNI is respected: no extra assemblies, no mediator, no generic repository.

Negative:

- The compiler cannot prevent a Domain file from `using` Infrastructure in the same project. Review must catch this until assemblies are split or architecture tests exist.
- Folder-level Clean Architecture is weaker than project-level isolation. That is accepted until a second deployable or a second team needs a hard boundary.

Follow-up (not in this checkpoint):

- Split assemblies when a second project needs to reference Domain without pulling the Worker.
- Add architecture tests that fail the build on forbidden `using` directions.
