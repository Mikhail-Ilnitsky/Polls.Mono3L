using Ilnitsky.Polls.BusinessLogic.Handlers.Polls;
using Ilnitsky.Polls.Contracts.Dtos.Polls;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ilnitsky.Polls.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class PollsController : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<PollLinkDto>> Get(
        [FromQuery] int? offset,
        [FromQuery] int? limit,
        [FromServices] GetPollLinksHandler handler)
    {
        offset ??= 0;
        limit ??= 5;
        return await handler.HandleAsync(offset.Value, limit.Value);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PollDto>> Get(
        [FromRoute] Guid id,
        [FromServices] GetPollByIdHandler handler)
    {
        // HACK: тестовое исключение для несуществующего id
        if (id == Guid.Parse("019c1aa8-9bf0-750d-9e6d-832de94b1c13"))
        {
            throw new Exception("Тестовое исключение!");
        }

        var poll = await handler.HandleAsync(id);

        return poll.GetActionResult(HttpContext);
    }
}
