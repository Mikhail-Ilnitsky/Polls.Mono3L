using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using System.Text.Json;

namespace Ilnitsky.Polls.Filters;

public class ErrorResultFilter : IAlwaysRunResultFilter
{
    public void OnResultExecuting(ResultExecutingContext executingContext)
    {
        if (!executingContext.ModelState.IsValid)
        {
            var errors = executingContext
                .ModelState
                .Where(pair => pair.Value?.Errors.Count > 0)
                .ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value!.Errors
                        .Select(e => e.ErrorMessage)
                        .ToArray()
                );

            executingContext.HttpContext.Items["ModelErrors"] = JsonSerializer.Serialize(errors);
        }
        else if (executingContext.Result is ObjectResult result && result.StatusCode >= 400)
        {
            executingContext.HttpContext.Items["BadResult"] = JsonSerializer.Serialize(result.Value);
        }
    }

    public void OnResultExecuted(ResultExecutedContext executingContext) { }
}
