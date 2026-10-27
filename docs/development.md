# Development

## Prerequisites

- .NET SDK 9.0.100 or later (`global.json` rolls forward on latest minor)
- Optional: SQL Server if you switch `Database:Provider`

```powershell
dotnet --list-sdks
```

## Build and run

```powershell
dotnet restore
dotnet build
dotnet run --project src/NetMinimal.Api
```

HTTP profile: http://localhost:5088 (launches Scalar).  
HTTPS profile: https://localhost:7088.

## Tests and coverage collector

```powershell
dotnet test
```

`coverlet.collector` is referenced on the test project if you add a coverage run:

```powershell
dotnet test --collect:"XPlat Code Coverage"
```

## EF Core tools

```powershell
dotnet tool update --global dotnet-ef
dotnet ef migrations add <Name> --project src/NetMinimal.Api
dotnet ef migrations list --project src/NetMinimal.Api
dotnet ef database update --project src/NetMinimal.Api
```

Do not hand-edit snapshot files. Change fluent configuration, then add a migration.

## Code rules

`Directory.Build.props` sets:

- `net9.0`
- nullable enabled
- `TreatWarningsAsErrors`
- latest analysis level

A warning is a failed build. Prefer fixing the cause over `#pragma`.

## Adding an endpoint

1. Contract + validator under `Application/`.
2. Service method + unit tests.
3. Map the route in `*Endpoints.cs` under `/api/v1`.
4. Attach `ValidationFilter<T>` for body types.
5. Document it in `docs/api.md` and add a sample to `docs/http/catalog.http`.
6. HTTP test if the status code mapping matters.

## Local database reset

Stop the API, delete `src/NetMinimal.Api/catalog.dev.db` (and `-wal`/`-shm`), start again. Seeding runs because the categories table is empty.

## Solution structure conventions

- One web project (`NetMinimal.Api`) plus tests
- File-scoped namespaces
- Primary constructors for services
- Records for DTOs
- Folders by concern (`Endpoints`, `Application`, `Domain`, `Infrastructure`)
- No MediatR / no extra frameworks — services are the application boundary
