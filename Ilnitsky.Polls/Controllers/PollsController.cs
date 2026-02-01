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
    public async Task<IEnumerable<PollDto>> Get(
        [FromQuery] int? offset,
        [FromQuery] int? limit,
        [FromServices] GetPollsHandler handler)
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
        var poll = await handler.HandleAsync(id);

        return poll is null
            ? NotFound()
            : Ok(poll);
    }
}
