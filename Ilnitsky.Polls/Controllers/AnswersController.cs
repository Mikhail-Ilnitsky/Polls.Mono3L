using Ilnitsky.Polls.BusinessLogic.Handlers.Answers;
using Ilnitsky.Polls.Contracts.Dtos.Answers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Ilnitsky.Polls.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AnswersController : ControllerBase
    {
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post(
            [FromBody] CreateRespondentAnswerDto answerDto,
            [FromServices] CreateRespondentAnswerHandler handler)
        {
            var respondentIdString = HttpContext.Session.GetString("RespondentId");
            var respondentSessionIdString = HttpContext.Session.GetString("RespondentSessionId");

            if (respondentIdString is null || !Guid.TryParse(respondentIdString, out var _))
            {
                HttpContext.Items["ErrorDetails"] = $"Некорректное значение respondentId = '{respondentIdString}'";
                return BadRequest("Некорректный идентификатор респондента!");
            }
            if (respondentSessionIdString is null || !Guid.TryParse(respondentSessionIdString, out var _))
            {
                HttpContext.Items["ErrorDetails"] = $"Некорректное значение respondentSessionId = '{respondentSessionIdString}'";
                return BadRequest("Некорректный идентификатор сессии респондента!");
            }

            var respondentId = Guid.Parse(respondentIdString);
            var respondentSessionId = Guid.Parse(respondentSessionIdString);

            var response = await handler.HandleAsync(answerDto, respondentSessionId, respondentId);

            return response.GetActionResult(HttpContext);
        }
    }
}
