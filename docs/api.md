# HTTP API

Base path: **`/api/v1`**. JSON request and response bodies. Dates are ISO-8601 UTC offsets.

Interactive docs: http://localhost:5088/scalar  
Machine-readable: http://localhost:5088/openapi/v1.json  
Copy-paste samples: [http/catalog.http](http/catalog.http)

Root `GET /` redirects to `/scalar`.

## Categories

### `GET /api/v1/categories`

List categories.

| Query | Type | Default | Description |
| --- | --- | --- | --- |
| `includeInactive` | bool | `false` | Include deactivated categories |

**200** — array of `CategoryResponse`.

### `GET /api/v1/categories/{id}`

**200** — one category. **404** — `not_found`.

### `POST /api/v1/categories`

```json
{ "name": "Accessories", "description": "Cables and cases" }
```

**201** — body is the created category; `Location` is `/api/v1/categories/{id}`.  
**400** — validation. **409** — slug already used (`conflict`).

### `PUT /api/v1/categories/{id}`

```json
{ "name": "Accessories", "description": "Cables", "isActive": true }
```

**200** — updated category. **400** / **404** / **409**.

### `DELETE /api/v1/categories/{id}`

**204** — deleted. **404**. **409** — category still has products.

## Products

### `GET /api/v1/products`

| Query | Type | Default | Description |
| --- | --- | --- | --- |
| `search` | string | | Case-sensitive contains on `Name` or `Sku` (SQLite default collation) |
| `categoryId` | guid | | Filter to one category |
| `includeInactive` | bool | `false` | Include deactivated products |
| `page` | int | `1` | `1..10000` |
| `pageSize` | int | `25` | `1..100` |

**200** — paged envelope:

```json
{
  "items": [ ],
  "totalCount": 3,
  "page": 1,
  "pageSize": 25,
  "totalPages": 1,
  "hasNext": false,
  "hasPrevious": false
}
```

**422** — `page` / `pageSize` out of range (`business_rule`).

### `GET /api/v1/products/{id}`

**200** / **404**.

### `POST /api/v1/products`

```json
{
  "sku": "cam-9",
  "name": "USB Camera",
  "description": null,
  "price": 79.00,
  "stock": 4,
  "categoryId": "99615b26-52b2-4a26-b7af-3e71c5eeeafe"
}
```

SKU is stored as `CAM-9`. **201** + `Location`. **400** validation. **404** unknown category. **409** duplicate SKU. **422** inactive category or domain rule.

### `PUT /api/v1/products/{id}`

SKU cannot be changed.

```json
{
  "name": "USB Camera",
  "description": "1080p",
  "price": 69.00,
  "isActive": true,
  "categoryId": "99615b26-52b2-4a26-b7af-3e71c5eeeafe"
}
```

### `POST /api/v1/products/{id}/stock`

```json
{ "delta": -2 }
```

**200** — product with new stock. **400** if `delta` is 0. **422** if stock would go below zero.

### `DELETE /api/v1/products/{id}`

**204** / **404**.

## Health

`GET /health` — `200` and body `Healthy` when the database check passes. Used by orchestrators.

## CORS

Browser origins listed in `Cors:AllowedOrigins` may call the API with any header/method. Other origins are rejected. This is a default policy applied with `UseCors()`.

## Versioning

The `v1` prefix is the version. Add `/api/v2` as another `MapGroup` when the contract breaks; keep v1 handlers until clients migrate.
