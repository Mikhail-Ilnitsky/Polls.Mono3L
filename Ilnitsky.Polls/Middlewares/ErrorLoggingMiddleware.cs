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

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);

            var statusCode = context.Response.StatusCode;
            if (statusCode >= 400)
            {
                _logger.LogWarning(
                    "Ошибка {0} для {1} {2}: {3}",
                    statusCode,
                    context.Request.Method,
                    context.Request.Path,
                    context.Items["ErrorDetails"]);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Исключение для {0} {1}: {2}",
                context.Request.Method,
                context.Request.Path,
                ex.Message);

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await context.Response.WriteAsync("Внутренняя ошибка сервера");
        }
    }
}
