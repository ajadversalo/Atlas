# Testing

All tests live in `tests/NetMinimal.Tests` (xUnit + FluentAssertions).

```powershell
dotnet test
dotnet test --filter FullyQualifiedName~Domain
dotnet test --filter FullyQualifiedName~ProductServiceTests
```

Parallelization is **disabled** for the assembly because `AppServices` is process-wide.

## Layers

| Folder | Kind | I/O |
| --- | --- | --- |
| `Domain/` | Pure unit tests | None |
| `Application/` | Service tests | In-memory SQLite (one connection, `EnsureCreated`) |
| `Infrastructure/` | Repository, seeder, locator, clock | In-memory SQLite |
| `Api/` | HTTP via `WebApplicationFactory<Program>` | Temp SQLite file + migrations + seed |

## Support types

- `FakeClock` — deterministic `IClock`
- `SqliteCatalogFixture` — opens `Data Source=:memory:`, keeps the connection alive, exposes `Db`, `Products`, `Categories`, `Clock`

The in-memory SQLite database exists only while the connection is open; the fixture must not close it before assertions.

## What the tests cover

**Domain**

- `Guard` (null, whitespace, length, Guid, range, optional)
- `Clock.Freeze` isolation
- `SlugGenerator` (spaces, punctuation, diacritics, empty-ish)
- `Text.NormalizeSku`
- `Product` create/update/stock/price rounding and rejections
- `Category` create/update/slug
- `Entity` equality
- Exception codes

**Application**

- Product create, duplicate SKU, missing/inactive category, update, delete, query paging/search, stock adjust
- Category create, duplicate slug, delete with/without products, list inactive
- FluentValidation rules
- `PagedResult` page math
- DTO mapping

**Infrastructure**

- Product/category repository queries
- Seeder idempotency
- `SystemClock` returns UTC
- `AppServices` initialize / resolve / reset

**API**

- `/health`
- Seeded list
- Create product
- 404 ProblemDetails
- 400 validation

## Adding a unit test

1. Domain rule → `tests/NetMinimal.Tests/Domain`. No DbContext.
2. Service rule → new `[Fact]` in `*ServiceTests`, `await using var fx = new SqliteCatalogFixture()`.
3. HTTP contract → `CatalogApiTests` with `factory.CreateClient()`.

Name tests as `Method_scenario_expectedResult` so `dotnet test` output reads as documentation.

## HTTP factory notes

`CatalogApiFactory` writes a unique temp `.db` file, sets `MigrateOnStartup` and `SeedOnStartup`, and deletes the file on dispose. It uses the real `Program` pipeline (exception handler, filters, DI).
