using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Exceptions;

namespace TaskManager.Api.Handlers;

public class GlobalExceptionHandler(IProblemDetailsService problemDetails, ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            BusinessRuleException => (StatusCodes.Status400BadRequest, "Bad request"),
            NotFoundException     => (StatusCodes.Status404NotFound, "Not found"),
            ConflictException     => (StatusCodes.Status409Conflict, "Conflict"),
            _                     => (StatusCodes.Status500InternalServerError, "Internal server error")
        };

        // Only our own exceptions carry a message written for the client; anything else stays in the log.
        var isKnown = status != StatusCodes.Status500InternalServerError;
        if (!isKnown)
            logger.LogError(exception, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);

        context.Response.StatusCode = status;

        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = isKnown ? exception.Message : "An unexpected error occurred.",
                Instance = context.Request.Path
            }
        });
    }
}
