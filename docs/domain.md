# Domain model

The domain is persistence-ignorant. Entities expose factory methods and mutating operations. EF Core uses a private parameterless constructor and field-backed collections.

## Entity base

`Entity` (`src/NetMinimal.Api/Domain/Entities/Entity.cs`)

| Member | Meaning |
| --- | --- |
| `Id` | `Guid`, assigned at construction (`Guid.NewGuid()`) |
| `CreatedAtUtc` | Set once on create |
| `UpdatedAtUtc` | Set on every mutating operation |
| Equality | Two entities are equal when their `Id` values match |

## Category

Factory: `Category.Create(name, description, utcNow)`

| Property | Rules |
| --- | --- |
| `Name` | Required, trimmed, max 120 |
| `Slug` | Generated from `Name` via `SlugGenerator.From` (lowercase, dashes, no diacritics) |
| `Description` | Optional, trimmed, max 2000, blank → `null` |
| `IsActive` | Defaults `true`; set on `Update` |
| `Products` | Read-only view of a private `List<Product>` |

`Update(name, description, isActive, utcNow)` regenerates the slug from the new name. Uniqueness of slug is enforced in **application** (`CategoryService`), not on the entity, because it needs a repository lookup.

## Product

Factory: `Product.Create(sku, name, description, price, stock, categoryId, utcNow)`

| Property | Rules |
| --- | --- |
| `Sku` | Required, trimmed, max 64, stored **uppercase** (`Text.NormalizeSku`) |
| `Name` | Required, trimmed, max 200 |
| `Description` | Optional, trimmed, max 4000 |
| `Price` | `>= 0`, rounded to 2 decimals (`MidpointRounding.AwayFromZero`) |
| `Stock` | `>= 0` |
| `CategoryId` | Non-empty `Guid` |
| `IsActive` | Defaults `true` |

Operations:

- `Update(...)` — name, description, price, active flag, category. **SKU is immutable** after create.
- `SetStock(stock, utcNow)` — absolute value, rejects negative.
- `AdjustStock(delta, utcNow)` — relative change; rejects a result below zero with `BusinessRuleException` including the SKU.

Category existence and “active category” are application rules: the entity only stores `CategoryId`.

## Exceptions

All inherit `AppException` and carry a stable `Code` for HTTP mapping.

| Type | Code | When |
| --- | --- | --- |
| `NotFoundException` | `not_found` | Aggregate missing by id |
| `ConflictException` | `conflict` | Duplicate SKU/slug, delete category with products |
| `BusinessRuleException` | `business_rule` | Guard failures, negative price/stock, inactive category |

See [Error handling](error-handling.md).

## Invariants to keep when changing code

1. Put **shape and range** rules on the entity (`Guard`, price, stock).
2. Put **uniqueness and existence** rules in the application service (they need I/O).
3. Never set entity properties from outside the type; keep setters `private`.
4. Pass `DateTimeOffset` into mutating methods so tests can freeze time via `IClock`.
