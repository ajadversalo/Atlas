# Application services

The application layer is the use-case boundary. Endpoints never talk to `DbContext` or entities directly.

## Ports (abstractions)

| Interface | Role |
| --- | --- |
| `IProductRepository` | Load/query/add/remove products |
| `ICategoryRepository` | Load/list/add/remove categories, `HasProductsAsync` |
| `IUnitOfWork` | `SaveChangesAsync` — implemented by `CatalogDbContext` |
| `IClock` | `UtcNow` — implemented by `SystemClock` (singleton) |

Repositories **do not** call `SaveChanges`. Services call `IUnitOfWork` after mutating the graph so a use case is one transaction.

## `IProductService`

| Method | Behavior |
| --- | --- |
| `QueryAsync(ProductQuery)` | Page `1..10000`, page size `1..100`. Filters: `Search` (name or SKU contains), `CategoryId`, `IncludeInactive`. Ordered by name. |
| `GetAsync(id)` | `NotFoundException` if missing |
| `CreateAsync` | Category must exist and be **active**. SKU unique (case-insensitive via normalization). |
| `UpdateAsync` | Category must exist and be active. SKU unchanged. |
| `AdjustStockAsync` | Delegates to `Product.AdjustStock` |
| `DeleteAsync` | Hard delete |

`ProductQuery` lives next to `IProductRepository` because both the service and the repository share it.

## `ICategoryService`

| Method | Behavior |
| --- | --- |
| `ListAsync(includeInactive)` | Active only unless the flag is true; ordered by name |
| `GetAsync(id)` | `NotFoundException` if missing |
| `CreateAsync` | Slug derived from name; slug must be unique |
| `UpdateAsync` | May rename (new slug) and deactivate |
| `DeleteAsync` | `ConflictException` if any products still reference the category |

## Contracts (DTOs)

Records in `NetMinimal.Application.Contracts`. JSON uses camelCase (ASP.NET Core default).

**Category**

```json
{
  "id": "99615b26-52b2-4a26-b7af-3e71c5eeeafe",
  "name": "Hardware",
  "slug": "hardware",
  "description": "Physical catalog items",
  "isActive": true,
  "createdAtUtc": "2026-10-27T20:10:53.6575601+00:00",
  "updatedAtUtc": "2026-10-27T20:10:53.6575601+00:00"
}
```

Create: `{ "name": "...", "description": "..." }`  
Update: `{ "name": "...", "description": "...", "isActive": true }`

**Product**

```json
{
  "id": "...",
  "sku": "KB-001",
  "name": "Mechanical Keyboard",
  "description": "Tactile switches",
  "price": 129.0,
  "stock": 25,
  "isActive": true,
  "categoryId": "...",
  "categoryName": "Hardware",
  "createdAtUtc": "...",
  "updatedAtUtc": "..."
}
```

Create includes `sku`, `name`, `description`, `price`, `stock`, `categoryId`.  
Update omits `sku`.  
Stock adjust: `{ "delta": 5 }` (`delta` must not be `0`).

## Mapping

`Mappings.ToResponse` extension methods copy entity fields onto response records. `categoryName` is `product.Category?.Name` and is populated when the repository `Include`s the navigation.

## Validation

FluentValidation validators are registered with `AddValidatorsFromAssembly` in `Program.cs`.

| Validator | Rules |
| --- | --- |
| `CreateCategoryRequestValidator` | Name required, max 120; description max 2000 |
| `UpdateCategoryRequestValidator` | Same |
| `CreateProductRequestValidator` | SKU/name required; price/stock `>= 0`; categoryId not empty |
| `UpdateProductRequestValidator` | Name required; price `>= 0`; categoryId not empty |
| `AdjustStockRequestValidator` | `Delta != 0` |

HTTP-layer `ValidationFilter<T>` turns failures into `400 ValidationProblem`. Domain `BusinessRuleException` still applies if something bypasses the filter.

## Adding a use case

1. Add or extend a contract record.
2. Add a validator if the type is a request body.
3. Add a method on the service interface + implementation.
4. Map an endpoint that injects the service.
5. Add a unit test in `tests/NetMinimal.Tests/Application`.
