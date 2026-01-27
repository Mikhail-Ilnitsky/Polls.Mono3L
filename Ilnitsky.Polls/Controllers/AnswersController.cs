using Ilnitsky.Polls.Contracts.Answers;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Ilnitsky.Polls.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AnswersController : ControllerBase
    {
        [HttpPost]
        public IActionResult Post([FromBody] RespondentAnswerDto answer)
        {
            Trace.WriteLine("Trace Answers POST:");
            Trace.WriteLine(answer);
            Trace.WriteLine(HttpContext);
            Trace.WriteLine("");
            return Ok();
        }
    }
}
