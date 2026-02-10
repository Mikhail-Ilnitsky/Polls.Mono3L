using Ilnitsky.Polls.Contracts.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ilnitsky.Polls.Controllers;

public static class Helper
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
            ErrorType.EntityNotFound => new NotFoundObjectResult(response.Message),
            ErrorType.IncorrectValue => new UnprocessableEntityObjectResult(response.Message),
            ErrorType.IncorrectFormat => new BadRequestObjectResult(response.Message),
            _ => new BadRequestObjectResult(response.Message)
        };
    }
}
