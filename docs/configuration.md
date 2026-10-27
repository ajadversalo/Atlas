# Configuration

Configuration is the ASP.NET Core stack: `appsettings.json` → `appsettings.{Environment}.json` → user secrets (Development) → environment variables → command line.

## Files

| File | Purpose |
| --- | --- |
| `src/NetMinimal.Api/appsettings.json` | Production-oriented defaults (SQLite `catalog.db`, no sensitive EF logging) |
| `src/NetMinimal.Api/appsettings.Development.json` | `catalog.dev.db`, EF command logging, sensitive data logging |
| User secrets | Connection strings with passwords (`UserSecretsId`: `netminimal-api-dev`) |

`Properties/launchSettings.json` sets `ASPNETCORE_ENVIRONMENT=Development` and URLs `http://localhost:5088` / `https://localhost:7088`.

## Connection string

Name: **`Catalog`**.

```json
"ConnectionStrings": {
  "Catalog": "Data Source=catalog.dev.db"
}
```

Overrides:

```powershell
dotnet user-secrets set "ConnectionStrings:Catalog" "Server=localhost,1433;Database=NetMinimal;User Id=sa;Password=...;TrustServerCertificate=True" --project src/NetMinimal.Api
```

```text
ConnectionStrings__Catalog=Data Source=/data/catalog.db
```

Missing `Catalog` fails fast in `Program.cs`.

## `Database` section

Bound to `NetMinimal.Infrastructure.Options.DatabaseOptions` (`SectionName = "Database"`).

| Key | Type | Default | Notes |
| --- | --- | --- | --- |
| `Provider` | `Sqlite` \| `SqlServer` | `Sqlite` | Required |
| `CommandTimeoutSeconds` | int | 30 | Range 1–300 |
| `MigrateOnStartup` | bool | true | `MigrateAsync` vs `EnsureCreated` |
| `SeedOnStartup` | bool | true | Idempotent catalog seed |
| `EnableSensitiveDataLogging` | bool | false | On in Development |
| `EnableDetailedErrors` | bool | false | On in Development |

Invalid options fail at host start (`ValidateOnStart` + data annotations).

Environment examples:

```text
Database__Provider=SqlServer
Database__MigrateOnStartup=true
Database__SeedOnStartup=false
```

## `Cors` section

Bound to `CorsSettings`.

```json
"Cors": {
  "AllowedOrigins": [
    "http://localhost:5173",
    "https://localhost:5173"
  ]
}
```

Empty list → default policy rejects every origin (`SetIsOriginAllowed(_ => false)`).

## Logging

Development logs EF SQL (`Microsoft.EntityFrameworkCore.Database.Command` = Information). Production `appsettings.json` keeps EF at Warning.

## HTTPS

`UseHttpsRedirection` is enabled. When running **only** HTTP (`--urls http://localhost:5088`) you may see `Failed to determine the https port for redirect`; HTTP endpoints still work.

## Switching to SQL Server

1. Set `Database:Provider` to `SqlServer` and a SQL Server connection string.
2. Generate SQL Server migrations (the checked-in ones are SQLite).
3. Run the API and confirm `/health` is Healthy.

Until SQL Server migrations exist, you can set `MigrateOnStartup` to `false` so `EnsureCreated` builds the schema (no migration history; not for production).
