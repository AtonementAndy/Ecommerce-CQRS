using Ecommerce.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.API.Middlewares
{
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var (statusCode, title, detail) = exception switch
            {
                DbUpdateConcurrencyException => (409, "Conflict",
                    "The resource was modified by another user. Please reload and try again."),
                NotFoundException => (404, "Resource Not Found", exception.Message),
                InvalidOperationException => (400, "Bad Request", exception.Message),
                _ => (500, "Internal Server Error", "An unexpected error occurred.")
            };

            // Loga em nível adequado: erros de negócio = Warning, erros inesperados = Error
            if (statusCode >= 500)
                logger.LogError(exception, "Unhandled exception on {Path}", httpContext.Request.Path);
            else
                logger.LogWarning(exception, "Business exception on {Path}: {Message}",
                    httpContext.Request.Path, exception.Message);

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
    }
}
