using Ilnitsky.Polls.Contracts.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ilnitsky.Polls.Controllers;

public static class ActionHelper
{
    public static ActionResult<TValue> GetActionResult<TValue>(this Response<TValue> response, HttpContext httpContext)
    {
        if (response.IsSuccess)
        {
            return new OkObjectResult(response.Value);
        }

        return GetError(response, httpContext);
    }

    public static IActionResult GetActionResult(this BaseResponse response, HttpContext httpContext)
    {
        if (response.IsSuccess && response.IsCreated)
        {
            return new CreatedResult("", response.Message);
        }
        if (response.IsSuccess)
        {
            return new OkObjectResult(response.Message);
        }

        return GetError(response, httpContext);
    }

    public static ActionResult GetError(IErrorInfo response, HttpContext httpContext)
    {
        httpContext.Items["ErrorDetails"] = $"{response.Message} {response.ErrorDetails}";

        return response.ErrorType switch
        {
            ErrorType.EntityNotFound => new NotFoundObjectResult(GetProblemDetails(404, response.Message)),
            ErrorType.IncorrectValue => new UnprocessableEntityObjectResult(GetProblemDetails(422, response.Message)),
            ErrorType.IncorrectFormat => new BadRequestObjectResult(GetProblemDetails(400, response.Message)),
            _ => new BadRequestObjectResult(GetProblemDetails(400, response.Message))
        };
    }

    public static ProblemDetails GetProblemDetails(int statusCode, string? message)
        => new ProblemDetails
        {
            Status = statusCode,
            Title = "Ошибка!",
            Detail = message,
        };
}
