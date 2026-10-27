# Static services

Two kinds of “static services” exist in this solution.

1. **Pure static helpers** under `Domain/StaticServices` — no container, no I/O.
2. **`AppServices` locator** under `Infrastructure/StaticServices` — a process-wide `IServiceProvider` for code that cannot take constructor injection.

Prefer constructor DI (`IClock`, repositories, services) in every new class that the container creates.

## `Guard`

`NetMinimal.Domain.StaticServices.Guard`

Argument checks that throw `BusinessRuleException`. Parameter names come from `[CallerArgumentExpression]`.

| Method | Behavior |
| --- | --- |
| `AgainstNull<T>` | Null reference → required |
| `AgainstNullOrWhiteSpace` | Trim; reject blank; optional `maxLength` |
| `Optional` | Blank → `null`; otherwise trim + max length |
| `AgainstEmpty` | Reject `Guid.Empty` |
| `AgainstOutOfRange` | Inclusive min/max for `int` (used for paging) |

Entities call `Guard` so invalid aggregates cannot be constructed.

## `Clock`

`NetMinimal.Domain.StaticServices.Clock`

```csharp
using var _ = Clock.Freeze(new DateTimeOffset(2026, 1, 15, 12, 0, 0, TimeSpan.Zero));
var now = Clock.UtcNow; // frozen
```

Uses `AsyncLocal`, so a freeze is isolated to the async flow (safe for tests). Production entity methods take `DateTimeOffset` from `IClock` instead of calling `Clock.UtcNow` directly.

## `SlugGenerator`

`SlugGenerator.From(name)`:

1. Trim / lowercase
2. Strip combining marks (é → e)
3. Replace non `[a-z0-9]` with `-`
4. Collapse dashes, trim edges
5. Empty result → `"item"`

Used when creating or renaming a category.

## `Text`

`Text.NormalizeSku` trims and uppercases. Products store SKUs this way; `GetBySkuAsync` normalizes the lookup key too.

## `AppServices`

`NetMinimal.Infrastructure.StaticServices.AppServices`

Initialized **once** after `builder.Build()`:

```csharp
AppServices.Initialize(app.Services);
```

| Member | Purpose |
| --- | --- |
| `Initialize` | Store the root provider |
| `IsInitialized` | Tests / diagnostics |
| `Clock` | `GetRequired<IClock>()` |
| `GetRequired<T>()` | Resolve from the root provider |
| `CreateScope()` | Resolve **scoped** services (DbContext, repositories) |

Resolving a scoped service from the root provider is incorrect. Always `CreateScope()` for those.

Use `AppServices` only from:

- static factories
- legacy helpers
- infrastructure that cannot be constructed by the container

Do **not** use it from `ProductService`, endpoints, or new application code.

`ResetForTests()` is `internal` and visible to `NetMinimal.Tests` so unit tests can clear the locator without affecting the public API.
