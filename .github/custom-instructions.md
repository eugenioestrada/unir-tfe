# GitHub Copilot Custom Instructions

## Coding Guidelines
- Write identifiers, comments, and log or exception messages in English.
- Prefer structured logging with named placeholders (e.g., `{RoomCode}`) instead of string interpolation.
- Preserve asynchronous patterns: propagate `CancellationToken` parameters and call `ConfigureAwait(false)` inside non-UI libraries.
- Use constructor injection and guard clauses (`ArgumentNullException`) for required dependencies.
- Model DTOs as `record` types and keep business logic inside domain entities or services, not DTOs.

## Architecture Reminders
- Respect the layered layout described in `docs/architecture.md`: Domain → Application → Infrastructure → Web. Infrastructure implementations should be consumed via interfaces declared in the Application layer.
- Keep `GameTribunal.AppHost` limited to Aspire orchestration concerns (resource wiring, environment configuration).
- When introducing new features, ensure Application services orchestrate domain entities and expose DTOs; Web projects should stay thin and rely on dependency injection.
- Align new decisions with the technology choices in `docs/technology.md` (ASP.NET Core, SignalR, EF Core, Blazor Server, .NET 10).

## Observability and Localization
- Emit log messages for meaningful lifecycle events (room creation, round transitions, collisions) using English text and structured placeholders.
- Avoid logging sensitive or personally identifiable data; prefer codes, identifiers, or aggregate counts.

## Documentation and Testing
- Update the relevant files under `docs/` whenever behavior, architecture, or technology decisions change so the TFG narrative stays accurate.
- Add or adjust xUnit tests alongside production changes (`GameTribunal.Application.Tests` for application-layer logic) to keep coverage aligned with new behaviors and to document edge cases.
