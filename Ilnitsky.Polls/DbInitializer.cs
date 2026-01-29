using Ilnitsky.Polls.BusinessLogic;
using Ilnitsky.Polls.DataAccess;
using Ilnitsky.Polls.DataAccess.Entities.Polls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ilnitsky.Polls;

public interface IDbInitializer : IDisposable
{
    Task InitDatabaseAsync();
}

public class DbInitializer(ApplicationDbContext _dbContext) : IDbInitializer
{
    private bool _isDisposed;

    public void Dispose()
    {
        if (!_isDisposed)
        {
            _dbContext.Dispose();

            _isDisposed = true;
        }

        GC.SuppressFinalize(this);
    }

    public async Task InitDatabaseAsync()
    {
        if (_dbContext.Polls.Any())
        {
            return;
        }

        _dbContext.Polls.AddRange(CreatePolls());

        await _dbContext.SaveChangesAsync();
    }

    public static Guid CreateGuidV7() => GuidHelper.CreateGuidV7();

    public static List<Answer> CreateAnswers(List<string> answers)
        => answers
            .Select(a => new Answer { Id = CreateGuidV7(), Text = a })
            .ToList();

    public static List<Poll> CreatePolls()
    {
        List<Poll> polls =
        [
            new Poll
            {
                Id = CreateGuidV7(),
                Name = "Марки китайских автомобилей",
                Html = "<img class=\"mb-1 w-100\" src=\"https://infotables.ru/images/avto/logo_auto/logo_china_auto.png\">",
                IsActive = true,
                Questions = [
                    new Question
                    {
                        Id = CreateGuidV7(),
                        Text = "Какие марки китайских автомобилей вы знаете?",
                        AllowCustomAnswer = false,
                        AllowMultipleChoice = true,
                        Number = 1,
                        TargetAnswer = null,
                        MatchNextNumber = null,
                        DefaultNextNumber = null,
                        Answers = CreateAnswers([
                            "Brilliance", "BYD", "Changan", "Chery", "Dongfeng",
                            "FAW", "Foton", "GAC", "Geely", "Great Wall",
                            "Hafei", "Haima", "Haval", "Hawtai", "JAC",
                            "Lifan", "Zotye",
                        ]),
                    },
                ],
            },
            new Poll
            {
                Id = CreateGuidV7(),
                Name = "Футбольные команды",
                Html = "",
                IsActive = true,
                Questions = [
                    new Question
                    {
                        Id = CreateGuidV7(),
                        Text = "За какую футбольную команду вы болеете?",
                        AllowCustomAnswer = false,
                        AllowMultipleChoice = false,
                        Number = 1,
                        TargetAnswer = null,
                        MatchNextNumber = null,
                        DefaultNextNumber = null,
                        Answers = CreateAnswers([
                            "Реал Мадрид", "Барселона", "Манчестер Юнайтед",
                            "Ливерпуль", "Бавария", "Манчестер Сити", "ПСЖ",
                        ]),
                    },
                ],
            },
            new Poll
            {
                Id = CreateGuidV7(),
                Name = "Чем заправлять оливье",
                Html = "<img class=\"mb-1 w-100\" src=\"https://avatars.mds.yandex.net/i?id=bc05f1be7ca98bc42e73968391df00edd8ce8846-11408895-images-thumbs&n=13\">",
                IsActive = true,
                Questions = [
                    new Question
                    {
                        Id = CreateGuidV7(),
                        Text = "Чем вы заправляете оливье?",
                        AllowCustomAnswer = true,
                        AllowMultipleChoice = false,
                        Number = 1,
                        TargetAnswer = null,
                        MatchNextNumber = null,
                        DefaultNextNumber = null,
                        Answers = CreateAnswers([
                            "Сметаной", "Майонезом", "Оливковым маслом",
                        ]),
                    },
                ],
            },
            new Poll
            {
                Id = CreateGuidV7(),
                Name = "Варка картошки",
                Html = "<img class=\"mb-1 w-100\" src=\"https://img.freepik.com/premium-photo/fresh-peeled-potatoes-wooden-table_220925-51555.jpg?semt=ais_hybrid&w=740&q=80\">",
                IsActive = true,
                Questions = [
                    new Question{
                        Id = CreateGuidV7(),
                        Text = "Как вы варите картошку?",
                        AllowCustomAnswer = true,
                        AllowMultipleChoice = false,
                        Number = 1,
                        TargetAnswer = null,
                        MatchNextNumber = null,
                        DefaultNextNumber = null,
                        Answers = CreateAnswers([
                            "В кожуре", "Почищенную", "Почищенную и порезанную",
                        ]),
                    },
                ],
            },
            new Poll
            {
                Id = CreateGuidV7(),
                Name = "Питание в школе",
                Html = "",
                IsActive = true,
                Questions = [
                    new Question{
                        Id = CreateGuidV7(),
                        Text = "Как ваши дети питаются в школе?",
                        AllowCustomAnswer = false,
                        AllowMultipleChoice = false,
                        Number = 1,
                        TargetAnswer = null,
                        MatchNextNumber = null,
                        DefaultNextNumber = null,
                        Answers = CreateAnswers([
                            "Платим за столовую в Кенгу.ру", "Покупает снеки в соседнем магазине", "Приносит еду из дома",
                        ]),
                    },
                ],
            },
            new Poll
            {
                Id = CreateGuidV7(),
                Name = "Брак и семья",
                Html = "<img class=\"mb-1 w-100\" src=\"https://st.depositphotos.com/1075946/3664/i/950/depositphotos_36646171-stock-photo-parents-with-children.jpg\">",
                IsActive = true,
                Questions = [
                    new Question
                    {
                        Id = CreateGuidV7(),
                        Text = "Сколько вам лет?",
                        AllowCustomAnswer = false,
                        AllowMultipleChoice = false,
                        Number = 1,
                        TargetAnswer = null,
                        MatchNextNumber = null,
                        DefaultNextNumber = 2,
                        Answers = CreateAnswers([
                            "младше 16", "от 16 до 17", "от 18 до 19", "от 20 до 21",
                            "от 22 до 24", "от 25 до 30", "от 31 до 35", "от 36 до 40",
                            "от 41 до 45", "от 46 до 50", "от 51 до 60", "от 61 до 70",
                            "от 71 до 80", "81 и больше",
                        ]),
                    },
                    new Question
                    {
                        Id = CreateGuidV7(),
                        Text = "Ваш пол?",
                        AllowCustomAnswer = false,
                        AllowMultipleChoice = false,
                        Number = 2,
                        TargetAnswer = null,
                        MatchNextNumber = null,
                        DefaultNextNumber = 3,
                        Answers = CreateAnswers([
                            "мужчина", "женщина",
                        ]),
                    },
                    new Question
                    {
                        Id = CreateGuidV7(),
                        Text = "В каком возрасте вы вступили в брак (в первый раз)?",
                        AllowCustomAnswer = false,
                        AllowMultipleChoice = false,
                        Number = 3,
                        TargetAnswer = "никогда не был(а) в браке",
                        MatchNextNumber = 7,
                        DefaultNextNumber = 4,
                        Answers = CreateAnswers([
                            "младше 16", "от 16 до 17", "от 18 до 19", "от 20 до 21", "от 22 до 24",
                            "от 25 до 30", "от 31 до 35", "от 36 до 40", "после 40", "никогда не был(а) в браке",
                        ]),
                    },
                    new Question
                    {
                        Id = CreateGuidV7(),
                        Text = "Вы развелись?",
                        AllowCustomAnswer = false,
                        AllowMultipleChoice = false,
                        Number = 4,
                        TargetAnswer = "да, в итоге развелись",
                        MatchNextNumber = 5,
                        DefaultNextNumber = 7,
                        Answers = CreateAnswers([
                            "да, в итоге развелись", "нет, не разводились",
                        ]),
                    },
                    new Question
                    {
                        Id = CreateGuidV7(),
                        Text = "Сколько раз вы были в браке?",
                        AllowCustomAnswer = false,
                        AllowMultipleChoice = false,
                        Number = 5,
                        TargetAnswer = "1",
                        MatchNextNumber = 7,
                        DefaultNextNumber = 6,
                        Answers = CreateAnswers([
                            "1", "2", "3", "4", "5", "6", "7", "больше 7",
                        ]),
                    },
                    new Question
                    {
                        Id = CreateGuidV7(),
                        Text = "Сейчас вы состоите в браке?",
                        AllowCustomAnswer = false,
                        AllowMultipleChoice = false,
                        Number = 6,
                        TargetAnswer = null,
                        MatchNextNumber = null,
                        DefaultNextNumber = 7,
                        Answers = CreateAnswers([
                            "да", "нет",
                        ]),
                    },
                    new Question
                    {
                        Id = CreateGuidV7(),
                        Text = "У вас есть родные дети?",
                        AllowCustomAnswer = false,
                        AllowMultipleChoice = false,
                        Number = 7,
                        TargetAnswer = "да",
                        MatchNextNumber = 8,
                        DefaultNextNumber = 10,
                        Answers = CreateAnswers([
                            "да", "нет",
                        ]),
                    },
                    new Question
                    {
                        Id = CreateGuidV7(),
                        Text = "Сколько у вас родных детей?",
                        AllowCustomAnswer = false,
                        AllowMultipleChoice = false,
                        Number = 8,
                        TargetAnswer = null,
                        MatchNextNumber = null,
                        DefaultNextNumber = 9,
                        Answers = CreateAnswers([
                            "1", "2", "3", "4", "5", "6", "7", "больше 7",
                        ]),
                    },
                    new Question
                    {
                        Id = CreateGuidV7(),
                        Text = "В каком возрасте у вас родился первый ребёнок?",
                        AllowCustomAnswer = false,
                        AllowMultipleChoice = false,
                        Number = 9,
                        TargetAnswer = null,
                        MatchNextNumber = null,
                        DefaultNextNumber = 10,
                        Answers = CreateAnswers([
                            "младше 16", "от 16 до 17", "от 18 до 19", "от 20 до 21",
                            "от 22 до 24", "от 25 до 30", "от 31 до 35", "от 36 до 40", "после 40",
                        ]),
                    },
                    new Question
                    {
                        Id = CreateGuidV7(),
                        Text = "У вас есть приёмные дети?",
                        AllowCustomAnswer = false,
                        AllowMultipleChoice = false,
                        Number = 10,
                        TargetAnswer = "да",
                        MatchNextNumber = 11,
                        DefaultNextNumber = null,
                        Answers = CreateAnswers([
                            "да", "нет",
                        ]),
                    },
                    new Question
                    {
                        Id = CreateGuidV7(),
                        Text = "Сколько у вас приёмных детей?",
                        AllowCustomAnswer = false,
                        AllowMultipleChoice = false,
                        Number = 11,
                        TargetAnswer = null,
                        MatchNextNumber = null,
                        DefaultNextNumber = null,
                        Answers = CreateAnswers([
                            "1", "2", "3", "4", "5", "6", "7", "больше 7",
                        ]),
                    },
                ],
            },
        ];

        return polls;
    }
}
