using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Exceptions;

namespace TaskManager.Api.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex) when (!context.Response.HasStarted)
        {
            var (status, title) = ex switch
            {
                BusinessRuleException => (StatusCodes.Status400BadRequest, "Bad request"),
                NotFoundException     => (StatusCodes.Status404NotFound, "Not found"),
                ConflictException     => (StatusCodes.Status409Conflict, "Conflict"),
                _                     => (StatusCodes.Status500InternalServerError, "Internal server error")            
            };

            // Only business exceptions carry a message meant for the client; anything else stays in the log.
            var isKnown = status != StatusCodes.Status500InternalServerError;
            if (!isKnown)
                logger.LogError(ex, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);

            var problem = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = isKnown ? ex.Message : "An unexpected error occurred.",
                Instance = context.Request.Path
            };

            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json");
        }
    }
}
