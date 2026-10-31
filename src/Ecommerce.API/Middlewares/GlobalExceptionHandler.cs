using Ecommerce.Application.Common.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.API.Middlewares;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, detail) = MapException(exception);

        // Log adequado: erro de negócio = Warning | erro inesperado = Error
        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception on {Path}", httpContext.Request.Path);
        }
        else
        {
            logger.LogWarning(exception, "Business exception on {Path}: {Message}",
                httpContext.Request.Path, exception.Message);
        }

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        }, cancellationToken);

        return true;
    }

    private static (int StatusCode, string Title, string Detail) MapException(Exception exception)
    {
        return exception switch
        {
            // 1. Exceptions de negócio (Application)
            NotFoundException => (
                StatusCodes.Status404NotFound,
                "Resource Not Found",
                exception.Message),

            ConflictException => (
                StatusCodes.Status409Conflict,
                "Conflict",
                exception.Message),

            // 2. FluentValidation
            ValidationException validationException => (
                StatusCodes.Status400BadRequest,
                "Validation Failed",
                string.Join("; ", validationException.Errors.Select(e => e.ErrorMessage))),

            // 3. Concorrência otimista (EF Core)
            DbUpdateConcurrencyException => (
                StatusCodes.Status409Conflict,
                "Conflict",
                "The resource was modified by another user. Please reload and try again."),

            // 4. Violação de constraint única (nome do produto duplicado)
            DbUpdateException dbEx when IsUniqueConstraintViolation(dbEx) => (
                StatusCodes.Status409Conflict,
                "Conflict",
                "A product with this name already exists."),

            // 5. Fallback
            _ => (
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "An unexpected error occurred.")
        };
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        var message = exception.InnerException?.Message ?? exception.Message;

        return message.Contains("IX_Products_Name", StringComparison.OrdinalIgnoreCase)
               || message.Contains("UNIQUE KEY", StringComparison.OrdinalIgnoreCase)
               || message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase)
               || message.Contains("unique index", StringComparison.OrdinalIgnoreCase);
    }
}