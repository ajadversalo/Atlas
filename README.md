# NetMinimal Catalog API

A single-project ASP.NET Core 9 **minimal API** for a product catalog. Folders hold entities, services, EF Core, and HTTP endpoints; tests live in a second project so they are not published with the app.

- constructor **dependency injection**
- an EF Core **database layer** with connection strings and provider switching
- application **services** and FluentValidation
- **static services** for helpers that cannot take DI
- RFC 7807 **ProblemDetails**, health checks, and OpenAPI / Scalar
- **xUnit** tests at the domain, service, repository, and HTTP layers

## Quick start

```powershell
dotnet restore
dotnet run --project src/NetMinimal.Api
```

| Surface | URL |
| --- | --- |
| API | http://localhost:5088 |
| Scalar UI | http://localhost:5088/scalar |
| OpenAPI JSON | http://localhost:5088/openapi/v1.json |
| Health | http://localhost:5088/health |

The first start applies SQLite migrations (`catalog.dev.db` in Development) and seeds two categories plus three products.

```powershell
dotnet test
```

## Documentation

| Guide | Contents |
| --- | --- |
| [Architecture](docs/architecture.md) | Folders, request flow, host bootstrap |
| [Domain model](docs/domain.md) | Entities, invariants, exceptions |
| [Application services](docs/application.md) | Use cases, contracts, mapping, validation |
| [Data layer](docs/data-layer.md) | EF Core, repositories, migrations, seeding |
| [HTTP API](docs/api.md) | Endpoints, query params, payloads, status codes |
| [Configuration](docs/configuration.md) | Connection strings, options, secrets, environments |
| [Dependency injection](docs/dependency-injection.md) | Registrations, lifetimes, how to add a service |
| [Static services](docs/static-services.md) | Guard, Clock, SlugGenerator, AppServices |
| [Error handling](docs/error-handling.md) | Exception types → HTTP ProblemDetails |
| [Testing](docs/testing.md) | Test layout, how to run, how to add tests |
| [Development](docs/development.md) | Local workflow, EF tools, SQL Server switch |
| [HTTP examples](docs/http/catalog.http) | Copy-paste requests for REST Client |

## Solution layout

```
.netMinimal/
  src/NetMinimal.Api            The app (folders for domain, services, data, HTTP)
  tests/NetMinimal.Tests        xUnit (unit + HTTP)
  docs/                         Detailed documentation
```

Inside the API project:

```
Endpoints/        HTTP maps
Domain/           Entities, exceptions, Guard / Clock / SlugGenerator
Application/      Services, DTOs, validators, repository interfaces
Infrastructure/   EF Core, repositories, options, AppServices
```

## Stack

- .NET 9 (`net9.0`), nullable reference types, warnings as errors
- ASP.NET Core minimal APIs + built-in OpenAPI + Scalar
- Entity Framework Core 9 (SQLite default, SQL Server optional)
- FluentValidation
- xUnit, FluentAssertions, `Microsoft.AspNetCore.Mvc.Testing`
