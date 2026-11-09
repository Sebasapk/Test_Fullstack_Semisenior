using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TaskManager.Api.Handlers;

public static class InvalidModelStateResponse
{
    public static IActionResult Create(ActionContext context)
    {
        var problem = context.HttpContext.RequestServices
            .GetRequiredService<ProblemDetailsFactory>()
            .CreateValidationProblemDetails(context.HttpContext, Sanitize(context.ModelState));

        return new BadRequestObjectResult(problem) { ContentTypes = { "application/problem+json" } };
    }

    // JSON parser errors (keys like "$.status") leak type names and line positions, so their message is replaced.
    private static ModelStateDictionary Sanitize(ModelStateDictionary modelState)
    {
        var safe = new ModelStateDictionary();
        foreach (var (key, entry) in modelState)
        {
            var isJsonError = key.StartsWith('$');
            foreach (var error in entry.Errors)
                safe.AddModelError(key, isJsonError ? "The value is invalid." : error.ErrorMessage);
        }
        return safe;
    }
}
