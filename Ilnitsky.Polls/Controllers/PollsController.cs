using Ilnitsky.Polls.BusinessLogic.Handlers.Polls;
using Ilnitsky.Polls.Contracts.Dtos.Polls;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ilnitsky.Polls.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class PollsController : ControllerBase
{
    private static readonly List<PollDto> _polls =
    [

    ];

    [HttpGet]
    public async Task<IEnumerable<PollDto>> Get(
        [FromServices] GetPollsHandler handler,
        [FromQuery] int? offset,
        [FromQuery] int? limit)
    {
        offset ??= 0;
        limit ??= 5;
        return await handler.HandleAsync(offset.Value, limit.Value);
    }

    [HttpGet("{id}")]
    public PollDto Get(Guid id)
    {
        return _polls.First(p => p.PollId == id);
    }

    [HttpPost]
    public IActionResult Post([FromBody] PollDto poll)
    {
        _polls.Add(poll);
        return Ok();
    }

    [HttpPut("{id}")]
    public IActionResult Put(Guid id, [FromBody] PollDto poll)
    {
        _polls.RemoveAll(p => p.PollId == id);
        _polls.Add(poll);
        return Ok();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(Guid id)
    {
        _polls.RemoveAll(p => p.PollId == id);
        return Ok();
    }
}
