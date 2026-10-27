# Data layer

The `Infrastructure/Data` folder owns EF Core, connection strings, and repository implementations.

## `CatalogDbContext`

- DbSets: `Products`, `Categories`
- Implements `IUnitOfWork` (`SaveChangesAsync`)
- Model built from `IEntityTypeConfiguration<>` in `Data/Configurations`

### Category mapping

- Table `Categories`
- Unique index on `Slug`
- `Name` 120, `Slug` 140, `Description` 2000
- Products collection uses **field** access (`_products`)
- Delete of a category with products is **Restrict** (FK)

### Product mapping

- Table `Products`
- Unique index on `Sku`
- Indexes on `CategoryId` and `Name`
- `Price` precision `(18, 2)`
- `Stock` / `IsActive` have store defaults

## Repositories

`ProductRepository` and `CategoryRepository` are scoped and take `CatalogDbContext`.

| Method | Notes |
| --- | --- |
| `GetByIdAsync` (product) | `Include(Category)` so responses have `categoryName` |
| `GetBySkuAsync` | Normalizes SKU before query |
| `QueryAsync` | `AsNoTracking`, filters, `Count` + `Skip/Take`, order by name |
| `ListAsync` (category) | `AsNoTracking`, optional active filter |
| `HasProductsAsync` | `AnyAsync` on products by `CategoryId` |

Writes use the **tracked** instance from `GetByIdAsync`. Query/list are no-tracking.

## Connection strings

The named connection is **`Catalog`**:

```json
"ConnectionStrings": {
  "Catalog": "Data Source=catalog.dev.db"
}
```

`AddCatalogServices` throws at startup if this value is missing.

`Database:Provider`:

| Value | EF call | Extra |
| --- | --- | --- |
| `Sqlite` (default) | `UseSqlite` | Local file; current migrations |
| `SqlServer` | `UseSqlServer` | Retry on failure (5 times, 10s), command timeout |

Other `Database` options: `CommandTimeoutSeconds` (1–300), `MigrateOnStartup`, `SeedOnStartup`, `EnableSensitiveDataLogging`, `EnableDetailedErrors`. Bound to `DatabaseOptions` with data annotations and **ValidateOnStart**.

## Startup initialization

`InitializeDatabaseAsync`:

1. If `MigrateOnStartup` → `Database.MigrateAsync()` (applies EF migrations).
2. Else → `EnsureCreatedAsync()` (schema without migration history; useful in some tests).
3. If `SeedOnStartup` → `CatalogSeeder.SeedAsync()`.

Seeder is **idempotent**: it returns immediately when any category exists.

Seeded rows:

| SKU | Name | Category |
| --- | --- | --- |
| `KB-001` | Mechanical Keyboard | Hardware |
| `MS-014` | Wireless Mouse | Hardware |
| `SW-200` | Pro License | Software |

## Migrations

Checked in under `src/NetMinimal.Api/Infrastructure/Data/Migrations`. They were generated for **SQLite**.

```powershell
dotnet ef migrations add <Name> --project src/NetMinimal.Api
dotnet ef database update --project src/NetMinimal.Api
```

Design-time factory: `CatalogDbContextFactory` uses `Data Source=catalog.db` so `dotnet ef` works without launching Kestrel.

SQL Server is wired in DI. Before using `MigrateOnStartup` against SQL Server, generate a SQL Server-specific migration (or a second migrations assembly). Mixing SQLite migration SQL with SQL Server will fail.

## SQLite files

Development uses `catalog.dev.db` next to the API project (see `appsettings.Development.json`). `*.db` is gitignored. Production `appsettings.json` uses `catalog.db`.
