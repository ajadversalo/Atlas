# Error handling

Unhandled exceptions pass through `IExceptionHandler` (`GlobalExceptionHandler`) and are written with `IProblemDetailsService`.

## Mapping

| Exception | HTTP | `title` | `code` | `detail` |
| --- | --- | --- | --- | --- |
| `NotFoundException` | 404 | Not Found | `not_found` | Exception message |
| `ConflictException` | 409 | Conflict | `conflict` | Exception message |
| `BusinessRuleException` | 422 | Business Rule Violated | `business_rule` | Exception message |
| `FluentValidation.ValidationException` | 400 | Validation Failed | `validation_failed` | Exception message |
| Anything else | 500 | Server Error | `internal_error` | `An unexpected error occurred.` |

`ValidationFilter<T>` returns **400 ValidationProblem** (`application/problem+json` with `errors`) **before** the handler runs. That path does not throw.

## ProblemDetails shape

```json
{
  "type": "https://httpstatuses.io/404",
  "title": "Not Found",
  "status": 404,
  "detail": "Product '3fa85f64-5717-4562-b3fc-2c963f66afa6' was not found.",
  "code": "not_found",
  "traceId": "00-..."
}
```

`traceId` is added in `AddProblemDetails` from `HttpContext.TraceIdentifier`.

5xx responses hide the exception message from clients. The handler logs 5xx as **Error** and 4xx as **Information**.

## Typical 422 vs 400

- **400** — the JSON body failed FluentValidation (empty name, negative price on the request, `delta: 0`).
- **422** — the body was well-formed but the domain rejected it (stock would go negative, inactive category, `page` out of range).

## Typical 409

- Duplicate product SKU
- Duplicate category slug
- Deleting a category that still has products

The database also has unique indexes on `Sku` and `Slug`. The service checks first so the client gets a `ConflictException` instead of a raw EF update exception (which would be 500).
