---
applyTo: "src/**/*.cs"
---
# C# Code Quality and Style Guide

## 1. Purpose and Scope
- **Objective:** ensure every `.cs` file in the Pandorium repository meets architecture, maintainability, and quality standards aligned with the official .NET 10 guidance and the latest Code Analysis (CA/IDE) recommendations.
- **Coverage:** applies to all layers (Domain, Application, Infrastructure, Web, Tests) and must respect the separation defined in `docs/architecture.md`.
- **Shared responsibility:** contributors must follow these guidelines before opening a pull request; reviewers must block changes that violate this document.

## 2. Core Conventions
- **Language:** type/member names, comments, and log messages must be written in English. Functional documentation remains in Spanish.
- **Versions:** rely on features available in C# 14 / .NET 10.
- **Nullability:** keep `#nullable enable` at project level. Resolve nullable reference warnings (IDE, CS8600-CS8625) without suppression unless a documented justification exists.
- **Usings:** prefer `file-scoped namespaces` and the implicit/global `using` directives provided by the SDK. Order `using` statements alphabetically; avoid aliases unless necessary.
- **Explicit types:** use `var` only when the right-hand side makes the type obvious; keep explicit primitive types for literals (CA1508, IDE0007).
- **Immutability:** favor `record`, `record struct`, and `init`-only properties for DTOs and value objects. Avoid public setters on entities unless EF Core requires them.
- **Documentation:** add XML docs (`///`) to public and `internal` members exposed outside the assembly; describe parameters, return values, and exceptions.

## 3. Architecture and Separation of Concerns
- **Pure domain:** keep the Domain layer free of external dependencies, I/O, and global state. All logic must remain deterministic (see `docs/game-logic.md`).
- **Application orchestrator:** Application services coordinate domain entities and abstract infrastructure via interfaces. Avoid side effects beyond the defined contracts.
- **Decoupled infrastructure:** concrete implementations (EF Core, APIs, SignalR) live in Infrastructure and must satisfy Application-layer contracts.
- **Thin web layer:** controllers, hubs, and Blazor components should only transform DTOs and manage UI lifecycle. Do not expose domain entities or business rules directly.
- **Cross-dependencies:** lower layers must never reference higher layers. Verify every change with `dotnet build` to catch accidental references.

## 4. Detailed Design and Best Practices
- **Guard clauses:** call `ArgumentNullException.ThrowIfNull(parameter);` in public constructors/methods (CA1062).
- **Constructors:** use constructor injection for dependencies; avoid the `ServiceLocator` pattern. Keep private constructors only for ORM requirements.
- **Member order:** private fields, properties, constructors, public methods, protected methods, private methods. Group related members together.
- **Expressiveness:** avoid cryptic abbreviations. Collections should use plural names.
- **Modern patterns:** use `switch` expressions, `required` members, and collection expressions when they increase clarity.
- **Exceptions:** throw specific exception types (`InvalidOperationException`, `ArgumentException`, `DomainException`). Do not swallow generic exceptions without rethrowing (`catch (Exception)`).
- **Optional parameters:** prefer explicit overloads when semantics differ.

## 5. Asynchronous Patterns and Concurrency
- **Async methods:** end names with `Async`, return `Task`/`ValueTask`. Avoid `async void` except for UI event handlers.
- **Cancellation tokens:** all I/O-facing methods accept a `CancellationToken` and propagate it downstream (CA2016).
- **No blocking calls:** never use `.Result`, `.GetAwaiter().GetResult()`, `Task.Wait()`, or `Thread.Sleep()` in production code (CA2007).
- **ConfigureAwait:** call `ConfigureAwait(false)` in non-UI libraries; omit it in Blazor and UI tests.
- **Streams and I/O:** rely on `await using` for `IAsyncDisposable` implementations (CA2012) to guarantee resource cleanup.
- **Thread safety:** use thread-safe types (`ImmutableDictionary`, etc.) or explicit synchronization (`SemaphoreSlim`) for shared data.

# Pandorium C# Code Review Standards

## Purpose & Scope
- Guide Copilot code review for every `src/**/*.cs` file.
- Focus on architecture boundaries, async discipline, logging, security, and analyzer compliance.
- Block any change that violates these directives.

---

## Quick Priorities
- Flag any Domain code that references Application, Infrastructure, or Web projects; Domain must stay deterministic (see `docs/game-logic.md`).
- Reject synchronous waits on tasks or missing `CancellationToken` propagation in async flows.
- Require structured logging with `ILogger<T>` and placeholders like `{RoomCode}`; reject string concatenation in logs.
- Prevent merges while Code Analysis warnings remain or `WarningsAsErrors` would fail.
- Ensure new behaviors ship with matching tests and documentation updates (`docs/planning.md`, `docs/testing.md`, `docs/requirements.md`).

---

## Architecture Boundaries
- Keep Domain free from I/O, randomness, DI, or framework types.
- Ensure Application services orchestrate domain logic via interfaces only; forbid business rules in controllers, hubs, or UI components.
- Keep Infrastructure implementations behind Application contracts; never surface EF Core types, `HttpClient` responses, or SignalR hubs outside Infrastructure/Web.
- Fail reviews if lower layers reference higher ones or if DTOs expose domain entities directly.

## Naming & Style
- Use English identifiers, comments, and log text.
- Prefer file-scoped namespaces and SDK implicit/global `using` directives; delete unused imports.
- Use explicit types for primitives and ambiguous expressions; reserve `var` for obvious assignments.
- Model DTOs/value objects as `record` or `record struct` with `init`-only properties; expose setters only when EF Core needs them.
- Document public and externally visible `internal` members with XML comments.

## Language & Features
- Target C# 14 / .NET 10 features only.
- Keep `#nullable enable` and resolve nullable warnings without suppression unless justified inline with `// Justification:`.
- Apply guard clauses (`ArgumentNullException.ThrowIfNull`) to public entry points (CA1062).
- Favor modern constructs (switch expressions, collection expressions, `required` members) when they clarify intent.

## Async & Concurrency
- Append `Async` to async method names and return `Task` or `ValueTask`; forbid `async void` outside UI event handlers.
- Propagate `CancellationToken` through every async call chain (CA2016).
- Reject `.Result`, `.Wait()`, `.GetAwaiter().GetResult()`, and `Thread.Sleep()` in production code (CA2007).
- Call `ConfigureAwait(false)` in non-UI libraries; omit it in Blazor UI and tests.
- Use `await using` for `IAsyncDisposable` objects and thread-safe collections or synchronization (`SemaphoreSlim`) when sharing state.

## Dependency Injection & Resources
- Inject dependencies via constructors; disallow service locators or static singletons.
- Validate constructor parameters and configuration early; block null services, empty collections, or invalid options.
- Choose DI lifetimes (`Singleton`, `Scoped`, `Transient`) intentionally and note shared-state risks during review.
- Dispose `IDisposable` instances (CA2000) and use `IDbContextFactory` for EF Core in parallel flows.
- Access environment data or configuration through Infrastructure abstractions, not directly in Domain/Application.

## Security & Logging
- Sanitize and validate external input; reject unescaped interpolation in SQL, logs, or formatted strings (CA2100, CA2350).
- Standardize on `System.Text.Json` with shared `JsonSerializerOptions`; avoid `dynamic` unless required and documented.
- Scrub PII and secrets from logs; prefer identifiers like `{RoomCode}` or `{PlayerId}`.
- Log exceptions with `logger.LogError(ex, "Message with {Placeholder}")` and wrap external calls with resiliency policies (Polly retries/circuit breakers).

## Testing Expectations
- Add or update unit tests under `GameTribunal.Application.Tests` or `GameTribunal.Domain.Tests` for new or changed logic; xUnit with AutoFixture/Moq is preferred when helpful.
- Extend integration tests with `WebApplicationFactory` for SignalR/API changes.
- Cover UI-visible requirements with Playwright tests and keep documentation aligned with `docs/testing.md`.
- Run `dotnet test` and `dotnet format analyzers` before requesting review.

## Code Analysis Rules to Enforce

| Category | Rule | Enforcement |
|----------|------|-------------|
| Correctness | CA1062 | Guard all public inputs against `null`. |
| Correctness | CA1305/CA1309 | Specify `IFormatProvider`; prefer ordinal comparisons. |
| Correctness | CA2000/CA2012 | Dispose `IDisposable` resources; use `await using`. |
| Correctness | CA2016 | Propagate `CancellationToken`. |
| Concurrency | CA2018 | Avoid capturing the default synchronization context. |
| Design | CA1707/CA1710 | Remove underscores in public names; apply proper suffixes. |
| Design | CA1812/CA1822 | Mark members static when possible; remove unused instances. |
| Performance | CA1827/CA1828 | Use the efficient `Count`/`Any` overloads. |
| Performance | CA1848/IDE0076 | Prefer `LoggerMessage` or structured interpolation. |
| Security | CA2100/CA2116 | Block dynamic SQL and unparameterized queries. |
| Security | CA2350/CA2352 | Validate external data before risky calls. |
| Style | IDE0005/IDE0008 | Remove unused `using`; prefer explicit types when clearer. |
| Style | IDE0044/IDE0057 | Use `readonly` fields and simplified expressions. |
| Style | IDE0060/IDE0063 | Remove unused parameters; use simplified `using` statements. |

- Treat these rules as build-breaking (`WarningsAsErrors`). Justify any suppression inline with an issue reference.
- Revisit the rule set whenever the .NET SDK or analyzer packages update.

## Code Example

```csharp
public sealed class StartRoundHandler
{
	private readonly IRoomRepository roomRepository;

	public StartRoundHandler(IRoomRepository roomRepository)
	{
		ArgumentNullException.ThrowIfNull(roomRepository);
		this.roomRepository = roomRepository;
	}

	public async Task HandleAsync(RoomCode roomCode, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(roomCode);

		var room = await roomRepository
			.GetAsync(roomCode, cancellationToken)
			.ConfigureAwait(false);

		if (room is null)
		{
			throw new InvalidOperationException($"Room {roomCode} not found");
		}

		room.StartNextPhase();
	}
}
```

- Flag missing guard clauses, absent `ConfigureAwait(false)` in library code, or domain mutations triggered directly from the Web layer.

## Review Checklist
- Architecture boundaries intact; Domain remains pure and deterministic.
- Async patterns respected; no blocking calls or missing tokens.
- Structured logging and security hygiene enforced; no sensitive data leaks.
- Tests and documentation updated alongside code changes.
- Code analysis warnings cleared; suppressions include justification.

## Maintenance
- The architecture team owns this guide; raise an issue before editing.
- Review quarterly or after upgrading the .NET SDK/analyzers to capture new rules.

Following these directives keeps the Pandorium C# codebase consistent, reviewable, and production ready.
