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

## Documentation
- Update the relevant files under `docs/` whenever behavior, architecture, or technology decisions change so the TFG narrative stays accurate.
- When adding or modifying tests, reflect the changes in `docs/testing.md`.
- When implementing new functionality, update `docs/planning.md` to reflect the current project status.
- If other changes affect additional documentation files or `README.md`, update them accordingly.

## Testing
- Add or adjust xUnit tests alongside production changes (`GameTribunal.Application.Tests` for application-layer logic) to keep coverage aligned with new behaviors and to document edge cases.
- When implementing new functionality, update unit tests and UI tests to cover the new behavior; as a best practice, consider writing the tests before the implementation to validate the outcome.

## Mandatory Implementation Checklist
1. Redact unit tests and UI tests that describe the expected behavior before coding when possible.
2. Implement the feature respecting the layered architecture, dependency injection, and asynchronous patterns.
3. Update `docs/planning.md` to reflect the current project status and roadmap impacts.
4. Adjust or add unit tests and UI tests to cover the delivered behavior; ensure they pass locally.
5. Update `docs/testing.md` with any new or modified tests and document relevant scenarios.
6. Update any other impacted documentation files or `README.md` with the new behavior or decisions.
7. Review structured logging for meaningful lifecycle events and absence of sensitive data.
8. Perform a final check that all mandatory documentation and tests are committed alongside the implementation.
