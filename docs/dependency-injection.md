# Dependency injection

Everything that crosses a folder boundary is registered on `IServiceCollection` and consumed through constructors or endpoint parameters.

## Registration entry point

`AddCatalogServices(IConfiguration)` in `Extensions/ServiceCollectionExtensions.cs` is the single registration method. `Program.cs` calls it once.

It registers application services, EF Core, repositories, options, health checks, ProblemDetails, OpenAPI, and CORS.

## Lifetimes

| Service | Lifetime | Implementation |
| --- | --- | --- |
| `IProductService` | Scoped | `ProductService` |
| `ICategoryService` | Scoped | `CategoryService` |
| `IProductRepository` | Scoped | `ProductRepository` |
| `ICategoryRepository` | Scoped | `CategoryRepository` |
| `CatalogDbContext` | Scoped | EF |
| `IUnitOfWork` | Scoped | same instance as `CatalogDbContext` |
| `CatalogSeeder` | Scoped | used during startup scope |
| `IValidator<T>` | Scoped | FluentValidation assembly scan |
| `IClock` | **Singleton** | `SystemClock` |
| `IOptions<DatabaseOptions>` | Options | bound + validated |
| `IOptions<CorsSettings>` | Options | bound |
| `GlobalExceptionHandler` | Exception handler | `AddExceptionHandler<>` |

Scoped is required for `DbContext`. `IClock` is singleton because it is stateless.

`IUnitOfWork` is registered as:

```csharp
services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<CatalogDbContext>());
```

so a service that takes both `IProductRepository` and `IUnitOfWork` shares one context per request.

## How endpoints get services

Minimal API handlers list services as parameters. The framework resolves them from the request scope:

```csharp
private static async Task<IResult> GetAsync(
    Guid id,
    IProductService service,
    CancellationToken cancellationToken)
```

Do not resolve `IProductService` from `AppServices` in new endpoints.

## Adding a new service

1. Define an interface next to the other abstractions (`ISomethingService` or a repository port).
2. Implement it under `Application/Services` or `Infrastructure` if it needs I/O.
3. Register it in `AddCatalogServices` with the correct lifetime.
4. Inject the interface — never the concrete type — at the call site.

## Options

```csharp
services.AddOptions<DatabaseOptions>()
    .Bind(configuration.GetSection(DatabaseOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

`ValidateOnStart` means a bad `CommandTimeoutSeconds` or missing provider crashes the process at boot, not on the first request.

## Health checks

`AddHealthChecks().AddDbContextCheck<CatalogDbContext>("database")` runs `CanConnectAsync` against the same provider/connection as the app.

## What not to register

- Domain entities
- Static helper classes (`Guard`, `SlugGenerator`)
- EF configuration types
