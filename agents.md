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
- For each new or modified functional requirement, explicitly document the cross-reference between the requirement and its UI tests (file name, primary scenario, and expected result) inside `docs/testing.md`. If the requirement does not yet have a UI test, the delivery must not be approved.
- Update `docs/requirements.md` whenever new functionality is implemented or existing requirements change, ensuring traceability between requirements, features, and tests.
- Ensure documentation under `docs/` and `README.md` is authored in Spanish.

## Testing Expectations
- Expand xUnit coverage within `src/GameTribunal.Application.Tests/` for domain and application logic; mock infrastructure via interfaces.
- Maintain SignalR hub tests/integration tests as described in `docs/testing.md`.
- For UI changes, extend Playwright suites in `src/GameTribunal.Web.UI.Tests/` (accessibility, responsive, visual regression) and refresh screenshots if assertions depend on them.
- Run `dotnet test` (or targeted filters) before finalizing work; ensure deterministic, fast tests.
- Capture UI and marketing screenshots using real device viewports (Mobile M, Mobile L, Tablet, Laptop, TV 1080p, TV 4K) to validate non-functional requirements before submission; do not capture full screen screenshots.
- For each implemented or modified functional requirement, create or update at least one UI test (Playwright) that validates:
  - The primary flow of the requirement (happy path).
  - At least one boundary condition or relevant variant (if applicable).
  - The user-observable outcome (visible elements, states, navigation, messages).
  No functionality may merge without its minimum UI coverage and without tests passing locally and in CI.

## Workflow Checklist (must-do)
**Pre-implementation steps (before coding):**
1. Understand the task context in full, revisiting functional and non-functional constraints defined in `docs/requirements.md` and related design materials.
2. Map the affected layers, contracts, and test artefacts to confirm acceptance criteria prior to implementation.

**Coding steps (during implementation):**
1. Draft or update unit/UI tests that capture the intended behavior before or during implementation.
2. For each functional requirement (new, extended, or corrected), add or update its corresponding UI test before requesting review (minimum one primary scenario per requirement).
3. Implement features within the prescribed architecture, honoring DI patterns and async guidelines.

**Post-implementation steps (after coding):**
1. Refresh `docs/planning.md` with status and implications of delivered work.
2. Ensure all relevant unit tests and UI tests exist, are updated, and pass locally.
3. Capture test adjustments in `docs/testing.md` plus any other affected documentation or `README.md`, including the requirement-to-UI-test matrix.
4. Verify structured logging covers key lifecycle events without exposing sensitive data.
5. Confirm the solution builds and all automated tests succeed (`dotnet test`).
6. Before delivery, double-check that code, docs, and tests (including new UI tests) land together in the same change set.
7. Embed screenshots of the implemented features directly in the pull request description, showing the key states and user-visible outcomes.

## Quick Reference: Gameplay Essentials
- Room lifecycle and game phases: `Lobby → CaseVoting → Defense → DefenseVoting? → Scoring → Finished` (`docs/game-logic.md`).
- Accusation and defense rules, tie-breakers, and scoring are spelled out in `docs/game-logic.md`; adhere strictly to those invariants when altering gameplay.
- Requirements and edge cases are cataloged in `docs/requirements.md`; validate new work against this list and extend it when requirements evolve.

## Collaboration Tips
- Before altering shared contracts (DTOs, interfaces, enums), scan dependent projects (`GameTribunal.Application`, `.Web`, `.Infrastructure`) and update all consumers in one sweep.
- When introducing new cases, titles, or configuration knobs, centralize defaults in configuration or seed data rather than scattering literals.
- Keep the repo friendly for demonstrations: avoid breaking Aspire orchestration, ensure migrations/tests do not require unpublished secrets, and maintain quick start parity with `README.md`.
