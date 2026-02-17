using Ilnitsky.Polls.BusinessLogic.Handlers.Polls;
using Ilnitsky.Polls.Contracts.Dtos.Polls;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ilnitsky.Polls.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class PollsController : ControllerBase
{
    /// <summary>
    /// Получение списка опросов
    /// </summary>
    /// <param name="offset">Количество пропускаемых опросов</param>
    /// <param name="limit">Количество получаемых опросов</param>
    /// <response code="200">Получен список опросов</response>
    /// <response code="500">Ошибка сервера</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PollLinkDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IEnumerable<PollLinkDto>> Get(
        [FromQuery] int? offset,
        [FromQuery] int? limit,
        [FromServices] GetPollLinksHandler handler)
    {
        if (offset is null || offset < 0)
        {
            offset = 0;
        }

        if (limit is null || limit <= 0)
        {
            limit = 5;
        }

        return await handler.HandleAsync(offset.Value, limit.Value);
    }

    /// <summary>
    /// Получение опроса
    /// </summary>
    /// <param name="id">ID опроса</param>
    /// <response code="200">Опрос успешно найден</response>
    /// <response code="400">Неправильный тип параметра</response>
    /// <response code="404">Опрос не найден</response>
    /// <response code="500">Ошибка сервера</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PollDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
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
