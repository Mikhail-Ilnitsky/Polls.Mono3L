using Ilnitsky.Polls.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Ilnitsky.Polls.Middlewares;

public class ErrorLoggingMiddleware(
    RequestDelegate next,
    ILogger<ErrorLoggingMiddleware> logger)
{
    private readonly RequestDelegate _next = next ?? throw new ArgumentNullException(nameof(next));
    private readonly ILogger<ErrorLoggingMiddleware> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);

            if (httpContext.Items.TryGetValue("ErrorDetails", out var errorDetails))
            {
                _logger.LogWarning(
                    "Ошибка {0} для {1} {2}: {3}",
                    httpContext.Response.StatusCode,
                    httpContext.Request.Method,
                    httpContext.Request.Path,
                    errorDetails);
            }
            else if (httpContext.Items.TryGetValue("ModelErrors", out var modelErrors))
            {
                _logger.LogWarning(
                    "Ошибка {0} для {1} {2}: {3}",
                    httpContext.Response.StatusCode,
                    httpContext.Request.Method,
                    httpContext.Request.Path,
                    modelErrors);
            }
            else if (httpContext.Items.TryGetValue("BadResult", out var badResult))
            {
                _logger.LogWarning(
                    "Ошибка {0} для {1} {2}: {3}",
                    httpContext.Response.StatusCode,
                    httpContext.Request.Method,
                    httpContext.Request.Path,
                    badResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Исключение для {0} {1}: {2}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                ex.Message);

            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await httpContext.Response
                .WriteAsJsonAsync(ActionHelper.GetProblemDetails(httpContext.Response.StatusCode, "Внутренняя ошибка сервера"));
        }
    }
}
