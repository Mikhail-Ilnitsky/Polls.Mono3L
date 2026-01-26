using Ilnitsky.Polls.Contracts.Polls;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ilnitsky.Polls.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class PollsController : ControllerBase
{
    static readonly List<PollDto> _polls = new()
    {
        new PollDto(
            Guid.NewGuid(),
            "Марки китайских автомобилей",
            "<img class=\"mb-1 w-100\" src=\"https://infotables.ru/images/avto/logo_auto/logo_china_auto.png\">",
            true,
            new List<QuestionDto>
            {
                new QuestionDto(
                    Guid.NewGuid(),
                    "Какие марки китайских автомобилей вы знаете?",
                    false,
                    true,
                    1,
                    null,
                    null,
                    null,
                    new List<string>
                    {
                        "Brilliance", "BYD", "Changan", "Chery", "Dongfeng", "FAW", "Foton", "GAC", "Geely",
                        "Great Wall", "Hafei", "Haima", "Haval", "Hawtai", "JAC", "Lifan", "Zotye",
                    }),
            }),
        new PollDto(
            Guid.NewGuid(),
            "Чем заправлять оливье",
            "<img class=\"mb-1 w-100\" src=\"https://avatars.mds.yandex.net/i?id=bc05f1be7ca98bc42e73968391df00edd8ce8846-11408895-images-thumbs&n=13\">",
            true,
            new List<QuestionDto>
            {
                new QuestionDto(
                    Guid.NewGuid(),
                    "Чем вы заправляете оливье?",
                    true,
                    false,
                    1,
                    null,
                    null,
                    null,
                    new List<string>
                    {
                        "Сметаной", "Майонезом", "Оливковым маслом",
                    }),
            }),
        new PollDto(
            Guid.NewGuid(),
            "Питание в школе",
            "",
            true,
            new List<QuestionDto>
            {
                new QuestionDto(
                    Guid.NewGuid(),
                    "Как ваши дети питаются в школе?",
                    false,
                    false,
                    1,
                    null,
                    null,
                    null,
                    new List<string>
                    {
                        "Платим за столовую в Кенгу.ру", "Покупает снеки в соседнем магазине", "Приносит еду из дома",
                    }),
            }),
    };

    [HttpGet]
    public IEnumerable<PollDto> Get()
    {
        return _polls.Where(p => p.IsActive);
    }

    [HttpGet("{id}")]
    public PollDto Get(Guid id)
    {
        return _polls.First(p => p.PollId == id);
    }

    [HttpPost]
    public void Post([FromBody] PollDto poll)
    {
        _polls.Add(poll);
    }

    [HttpPut("{id}")]
    public void Put(Guid id, [FromBody] PollDto poll)
    {
        _polls.RemoveAll(p => p.PollId == id);
        _polls.Add(poll);
    }

    [HttpDelete("{id}")]
    public void Delete(Guid id)
    {
        _polls.RemoveAll(p => p.PollId == id);
    }
}
