using Microsoft.AspNetCore.Diagnostics;
using NetMinimal.Domain.Exceptions;

namespace NetMinimal.Api.Middleware;

public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title, code) = exception switch
        {
            NotFoundException ex => (StatusCodes.Status404NotFound, "Not Found", ex.Code),
            ConflictException ex => (StatusCodes.Status409Conflict, "Conflict", ex.Code),
            BusinessRuleException ex => (StatusCodes.Status422UnprocessableEntity, "Business Rule Violated", ex.Code),
            FluentValidation.ValidationException => (StatusCodes.Status400BadRequest, "Validation Failed", "validation_failed"),
            _ => (StatusCodes.Status500InternalServerError, "Server Error", "internal_error")
        };

        if (status >= 500)
        {
            logger.LogError(exception, "Unhandled exception");
        }
        else
        {
            logger.LogInformation(exception, "Request failed with {StatusCode} ({Code})", status, code);
        }

        httpContext.Response.StatusCode = status;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails =
            {
                Status = status,
                Title = title,
                Detail = status >= 500 ? "An unexpected error occurred." : exception.Message,
                Type = $"https://httpstatuses.io/{status}",
                Extensions = { ["code"] = code }
            }
        });
    }
}
