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
    private static readonly List<PollDto> _polls =
    [
        new PollDto(
            Guid.NewGuid(),
            "Марки китайских автомобилей",
            "<img class=\"mb-1 w-100\" src=\"https://infotables.ru/images/avto/logo_auto/logo_china_auto.png\">",
            true,
            [
                new QuestionDto(
                    Guid.NewGuid(),
                    "Какие марки китайских автомобилей вы знаете?",
                    false,
                    true,
                    1,
                    null,
                    null,
                    null,
                    [
                        "Brilliance", "BYD", "Changan", "Chery", "Dongfeng",
                        "FAW", "Foton", "GAC", "Geely", "Great Wall",
                        "Hafei", "Haima", "Haval", "Hawtai", "JAC",
                        "Lifan", "Zotye",
                    ]),
            ]),
        new PollDto(
            Guid.NewGuid(),
            "Футбольные команды",
            "",
            true,
            [
                new QuestionDto(
                    Guid.NewGuid(),
                    "За какую футбольную команду вы болеете?",
                    false,
                    false,
                    1,
                    null,
                    null,
                    null,
                    [
                        "Реал Мадрид", "Барселона", "Манчестер Юнайтед",
                        "Ливерпуль", "Бавария", "Манчестер Сити", "ПСЖ",
                    ]),
            ]),
        new PollDto(
            Guid.NewGuid(),
            "Чем заправлять оливье",
            "<img class=\"mb-1 w-100\" src=\"https://avatars.mds.yandex.net/i?id=bc05f1be7ca98bc42e73968391df00edd8ce8846-11408895-images-thumbs&n=13\">",
            true,
            [
                new QuestionDto(
                    Guid.NewGuid(),
                    "Чем вы заправляете оливье?",
                    true,
                    false,
                    1,
                    null,
                    null,
                    null,
                    [
                        "Сметаной", "Майонезом", "Оливковым маслом",
                    ]),
            ]),
        new PollDto(
            Guid.NewGuid(),
            "Варка картошки",
            "<img class=\"mb-1 w-100\" src=\"https://img.freepik.com/premium-photo/fresh-peeled-potatoes-wooden-table_220925-51555.jpg?semt=ais_hybrid&w=740&q=80\">",
            true,
            [
                new QuestionDto(
                    Guid.NewGuid(),
                    "Как вы варите картошку?",
                    true,
                    false,
                    1,
                    null,
                    null,
                    null,
                    [
                        "В кожуре", "Почищенную", "Почищенную и порезанную",
                    ]),
            ]),
        new PollDto(
            Guid.NewGuid(),
            "Питание в школе",
            "",
            true,
            [
                new QuestionDto(
                    Guid.NewGuid(),
                    "Как ваши дети питаются в школе?",
                    false,
                    false,
                    1,
                    null,
                    null,
                    null,
                    [
                        "Платим за столовую в Кенгу.ру", "Покупает снеки в соседнем магазине", "Приносит еду из дома",
                    ]),
            ]),
        new PollDto(
            Guid.NewGuid(),
            "Брак и семья",
            "<img class=\"mb-1 w-100\" src=\"https://st.depositphotos.com/1075946/3664/i/950/depositphotos_36646171-stock-photo-parents-with-children.jpg\">",
            true,
            [
                new QuestionDto(
                    Guid.NewGuid(),
                    "Сколько вам лет?",
                    false,
                    false,
                    1,
                    null,
                    null,
                    2,
                    [
                        "младше 16", "от 16 до 17", "от 18 до 19", "от 20 до 21",
                        "от 22 до 24", "от 25 до 30", "от 31 до 35", "от 36 до 40",
                        "от 41 до 45", "от 46 до 50", "от 51 до 60", "от 61 до 70",
                        "от 71 до 80", "81 и больше",
                    ]),
                new QuestionDto(
                    Guid.NewGuid(),
                    "Ваш пол?",
                    false,
                    false,
                    2,
                    null,
                    null,
                    3,
                    [
                        "мужчина", "женщина",
                    ]),
                new QuestionDto(
                    Guid.NewGuid(),
                    "В каком возрасте вы вступили в брак (в первый раз)?",
                    false,
                    false,
                    3,
                    "никогда не был(а) в браке",
                    7,
                    4,
                    [
                        "младше 16", "от 16 до 17", "от 18 до 19", "от 20 до 21", "от 22 до 24",
                        "от 25 до 30", "от 31 до 35", "от 36 до 40", "после 40", "никогда не был(а) в браке",
                    ]),
                new QuestionDto(
                    Guid.NewGuid(),
                    "Вы развелись?",
                    false,
                    false,
                    4,
                    "да, в итоге развелись",
                    5,
                    7,
                    [
                        "да, в итоге развелись", "нет, не разводились",
                    ]),
                new QuestionDto(
                    Guid.NewGuid(),
                    "Сколько раз вы были в браке?",
                    false,
                    false,
                    5,
                    "1",
                    7,
                    6,
                    [
                        "1", "2", "3", "4", "5", "6", "7", "больше 7",
                    ]),
                new QuestionDto(
                    Guid.NewGuid(),
                    "Сейчас вы состоите в браке?",
                    false,
                    false,
                    6,
                    null,
                    null,
                    7,
                    [
                        "да", "нет",
                    ]),
                new QuestionDto(
                    Guid.NewGuid(),
                    "У вас есть родные дети?",
                    false,
                    false,
                    7,
                    "да",
                    8,
                    10,
                    [
                        "да", "нет",
                    ]),
                new QuestionDto(
                    Guid.NewGuid(),
                    "Сколько у вас родных детей?",
                    false,
                    false,
                    8,
                    null,
                    null,
                    9,
                    [
                        "1", "2", "3", "4", "5", "6", "7", "больше 7",
                    ]),
                new QuestionDto(
                    Guid.NewGuid(),
                    "В каком возрасте у вас родился первый ребёнок?",
                    false,
                    false,
                    9,
                    null,
                    null,
                    10,
                    [
                        "младше 16", "от 16 до 17", "от 18 до 19", "от 20 до 21",
                        "от 22 до 24", "от 25 до 30", "от 31 до 35", "от 36 до 40", "после 40",
                    ]),
                new QuestionDto(
                    Guid.NewGuid(),
                    "У вас есть приёмные дети?",
                    false,
                    false,
                    10,
                    "да",
                    11,
                    null,
                    [
                        "да", "нет",
                    ]),
                new QuestionDto(
                    Guid.NewGuid(),
                    "Сколько у вас приёмных детей?",
                    false,
                    false,
                    11,
                    null,
                    null,
                    null,
                    [
                        "1", "2", "3", "4", "5", "6", "7", "больше 7",
                    ]),
            ]),
    ];

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
