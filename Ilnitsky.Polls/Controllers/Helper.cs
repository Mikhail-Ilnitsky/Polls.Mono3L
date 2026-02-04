using Ilnitsky.Polls.Contracts.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Ilnitsky.Polls.Controllers;

public static class Helper
{
    public static ActionResult<TValue> GetActionResult<TValue>(this Response<TValue> response)
        => response.IsSuccess
                ? new OkObjectResult(response.Value)
            : response.ErrorType == ErrorType.EntityNotFound
                ? new NotFoundObjectResult(response.Message)
            : response.ErrorType == ErrorType.IncorrectValue
                ? new UnprocessableEntityObjectResult(response.Message)
                : new BadRequestObjectResult(response.Message);

    public static IActionResult GetActionResult(this BaseResponse response)
        => response.IsSuccess
                ? new OkObjectResult(response.Message)
            : response.ErrorType == ErrorType.EntityNotFound
                ? new NotFoundObjectResult(response.Message)
            : response.ErrorType == ErrorType.IncorrectValue
                ? new UnprocessableEntityObjectResult(response.Message)
                : new BadRequestObjectResult(response.Message);
}
