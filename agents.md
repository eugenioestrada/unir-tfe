# Agent Enablement Guide

## Context Snapshot
- Project name: Pandorium (GameTribunal solution).
- Goal: web-based party game for host screen + player mobiles; core value is social interaction through accusations, defenses, and humorous AI commentary.
- Primary stack: .NET 10, C#, ASP.NET Core, SignalR, Blazor Server, Entity Framework Core, optional Aspire orchestrator.
- Reference docs: `docs/architecture.md`, `docs/design.md`, `docs/game-logic.md`, `docs/requirements.md`, `docs/technology.md`, `docs/testing.md`, `.github/custom-instructions.md`.

## Architecture Guardrails
- Respect the layered flow Domain → Application → Infrastructure → Web (`docs/architecture.md`). Domain remains pure and deterministic; Application orchestrates domain + abstractions; Infrastruc[...]
- Keep the game engine deterministic: identical inputs must produce identical outcomes (`docs/game-logic.md`). Do not embed randomness, network calls, or UI concerns inside domain logic.
- SignalR hubs are the real-time entry point. Hubs delegate to Application services and return DTOs only; never surface domain entities directly.
- Any new infrastructure implementation must hang off an Application-layer interface. Configure wiring through dependency injection in `Program.cs` only.
- Infra may use EF Core and HttpClient, but must avoid leaking technology-specific types across boundaries.

## Coding Standards
- Follow `.github/custom-instructions.md` verbatim: English identifiers/comments, DTOs as `record`, constructor injection, guard clauses, propagate `CancellationToken`, call `ConfigureAwait(false)` in[...]
- Favor minimal, meaningful comments—use XML documentation on public members and add context only where the intent is non-obvious.
- Preserve asynchronous flows; avoid synchronously blocking asynchronous calls.
- When adding services, enforce null checks, validate parameters, and keep business logic inside domain or dedicated services (never in DTOs or controllers/components).

## Observability & Security
- Log meaningful lifecycle milestones (room creation, phase transitions, scoring, AI commentary fallback) with structured placeholders; omit sensitive or personal data.
- Ensure settings and secrets live in configuration (`appsettings*.json`, Secret Manager, environment variables) rather than source.
- Health checks and structured logging are planned; align additions with `docs/technology.md` guidance.

## Documentation Duties
- Converge behavior and documentation: update `docs/planning.md`, `docs/testing.md`, and any impacted architectural or design docs whenever functionality, strategy, or test coverage changes.
- Keep README concise; route detailed narratives to the relevant doc under `docs/`.
- Describe new or modified tests in `docs/testing.md`; reflect roadmap impacts in `docs/planning.md`.
- Para cada requisito funcional nuevo o modificado, documenta explícitamente la referencia cruzada entre el requisito y sus pruebas de UI (nombre del archivo, escenario principal y resultado esperado) dentro de `docs/testing.md`. Si el requisito no tiene todavía prueba de UI, no se debe aprobar la entrega.

## Testing Expectations
- Expand xUnit coverage within `src/GameTribunal.Application.Tests/` for domain and application logic; mock infrastructure via interfaces.
- Maintain SignalR hub tests/integration tests as described in `docs/testing.md`.
- For UI changes, extend Playwright suites in `src/GameTribunal.Web.UI.Tests/` (accessibility, responsive, visual regression) and refresh screenshots if assertions depend on them.
- Run `dotnet test` (or targeted filters) before finalizing work; ensure deterministic, fast tests.
- Por cada requisito funcional implementado o modificado se debe crear o actualizar al menos una prueba de UI (Playwright) que valide:
  - El flujo principal del requisito (happy path).
  - Al menos una condición límite o variante relevante (si aplica).
  - El resultado observable desde la perspectiva del usuario (elementos visibles, estados, navegación, mensajes).
  Ninguna funcionalidad se fusionará sin su cobertura mínima de UI y sin que las pruebas pasen localmente y en CI.

## Workflow Checklist (must-do)
1. Draft or update unit/UI tests that capture the intended behavior before or during implementation.
2. Para cada requisito funcional (nuevo, extendido o corregido), asegúrate de añadir o actualizar su prueba de UI correspondiente antes de solicitar revisión (mínimo un escenario principal por requisito).
3. Implement features within the prescribed architecture, honoring DI patterns and async guidelines.
4. Refresh `docs/planning.md` with status and implications of delivered work.
5. Ensure all relevant unit tests and UI tests exist, are updated, and pass locally.
6. Capture test adjustments in `docs/testing.md` plus any other affected documentation or `README.md`, incluyendo la matriz requisito → pruebas de UI.
7. Verify structured logging covers key lifecycle events without exposing sensitive data.
8. Confirm the solution builds and all automated tests succeed (`dotnet test`).
9. Before delivery, double-check that code, docs, and tests (incluyendo nuevas pruebas de UI) land together in the same change set.

## Quick Reference: Gameplay Essentials
- Room lifecycle and game phases: `Lobby → CaseVoting → Defense → DefenseVoting? → Scoring → Finished` (`docs/game-logic.md`).
- Accusation and defense rules, tie-breakers, and scoring are spelled out in `docs/game-logic.md`; adhere strictly to those invariants when altering gameplay.
- Requirements and edge cases are cataloged in `docs/requirements.md`; validate new work against this list and extend it when requirements evolve.

## Collaboration Tips
- Before altering shared contracts (DTOs, interfaces, enums), scan dependent projects (`GameTribunal.Application`, `.Web`, `.Infrastructure`) and update all consumers in one sweep.
- When introducing new cases, titles, or configuration knobs, centralize defaults in configuration or seed data rather than scattering literals.
- Keep the repo friendly for demonstrations: avoid breaking Aspire orchestration, ensure migrations/tests do not require unpublished secrets, and maintain quick start parity with `README.md`.
