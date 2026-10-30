using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.API.Middlewares
{
    public sealed class ConcurrencyExceptionHandler(
        ILogger<ConcurrencyExceptionHandler> logger)
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (exception is not DbUpdateConcurrencyException concurrencyException)
                return false;

            logger.LogWarning(
                concurrencyException,
                "Concurrency conflict detected for {Path}",
                httpContext.Request.Path);

            httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
            httpContext.Response.ContentType = "application/json";

            await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Detail = "The resource was modified by another user. Please reload and try again."
            }, cancellationToken);

            return true;
        }
    }
}
