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
        public async Task<IActionResult> Post(
            [FromBody] CreateRespondentAnswerDto answerDto,
            [FromServices] CreateRespondentAnswerHandler handler)
        {
            var respondentIdString = HttpContext.Session.GetString("RespondentId");
            var respondentSessionIdString = HttpContext.Session.GetString("RespondentSessionId");

            if (respondentIdString is null)
            {
                return BadRequest("Не передан id респондента!");
            }
            if (respondentSessionIdString is null)
            {
                return BadRequest("Не передан id сессии респондента!");
            }

            var respondentId = Guid.Parse(respondentIdString);
            var respondentSessionId = Guid.Parse(respondentSessionIdString);

            await handler.HandleAsync(answerDto, respondentSessionId, respondentId);

            return Ok("Ответ принят!");
        }
    }
}
