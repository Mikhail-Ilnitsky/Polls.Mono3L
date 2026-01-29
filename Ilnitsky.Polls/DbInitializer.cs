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
            new Poll
            {
                Id = CreateGuidV7(),
                Name = "Обеспеченность машинами",
                Html = null,
                IsActive = true,
                Questions = [
                    new Question{
                        Id = CreateGuidV7(),
                        Text = "Сколько всего людей в вашей семье?",
                        AllowCustomAnswer = false,
                        AllowMultipleChoice = false,
                        Number = 1,
                        TargetAnswer = "1",
                        MatchNextNumber = 3,
                        DefaultNextNumber = 2,
                        Answers = CreateAnswers([
                            "1", "2", "3", "4", "5", "6", "7", "8", "9",
                        ]),
                    },
                    new Question{
                        Id = CreateGuidV7(),
                        Text = "Сколько совершеннолетних в вашей семье?",
                        AllowCustomAnswer = false,
                        AllowMultipleChoice = false,
                        Number = 2,
                        TargetAnswer = null,
                        MatchNextNumber = null,
                        DefaultNextNumber = 3,
                        Answers = CreateAnswers([
                            "1", "2", "3", "4", "5", "6", "7", "8", "9",
                        ]),
                    },
                    new Question{
                        Id = CreateGuidV7(),
                        Text = "Сколько машин в вашей семье?",
                        AllowCustomAnswer = false,
                        AllowMultipleChoice = false,
                        Number = 3,
                        TargetAnswer = null,
                        MatchNextNumber = null,
                        DefaultNextNumber = null,
                        Answers = CreateAnswers([
                            "0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
                        ]),
                    },
                ],
            },
            new Poll
            {
                Id = CreateGuidV7(),
                Name = "Футбольные команды",
                Html = null,
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
                Name = "Питание в школе",
                Html = null,
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
                Name = "Хобби",
                Html = null,
                IsActive = true,
                Questions = [
                    new Question{
                        Id = CreateGuidV7(),
                        Text = "Как вы обычно проводите свободное время?",
                        AllowCustomAnswer = false,
                        AllowMultipleChoice = true,
                        Number = 1,
                        TargetAnswer = null,
                        MatchNextNumber = null,
                        DefaultNextNumber = null,
                        Answers = CreateAnswers([
                            "У меня не бывает свободного времени", "Смотрю телевизор", "Сижу в интернете", "Читаю книги",
                            "Играю на компьютере", "Играю на приставке", "Играю в настолки", "Играю в спортивные игры",
                            "Слушаю музыку", "Танцую", "Пою", "Хожу в походы", "Хожу в бассейн",
                            "Занимаюсь спортом дома", "Занимаюсь спортом на улице", "Занимаюсь спортом в спортзале",
                            "Хожу в кафе/рестораны/бары", "Пью дома", "Пью в гараже", "Пью на даче", "Пью в баре",
                            "Смотрю фильмы онлайн", "Смотрю сериалы онлайн", "Смотрю видосики",
                        ]),
                    },
                ],
            },
            new Poll
            {
                Id = CreateGuidV7(),
                Name = "Отпуск",
                Html = null,
                IsActive = true,
                Questions = [
                    new Question{
                        Id = CreateGuidV7(),
                        Text = "Как вы обычно проводите отпуск?",
                        AllowCustomAnswer = false,
                        AllowMultipleChoice = true,
                        Number = 1,
                        TargetAnswer = null,
                        MatchNextNumber = null,
                        DefaultNextNumber = null,
                        Answers = CreateAnswers([
                            "У меня не бывает отпусков", "Смотрю телевизор", "Читаю новости", "Читаю книги",
                            "Смотрю фильмы", "Смотрю сериалы", "Смотрю видосики", "Смотрю новости",
                            "Играю в видеоигры", "Играю в настолки", "Играю в спортивные игры",
                            "Хожу в походы", "Пью", "Ем", "Сплю", "Хожу в гости",
                            "Отдыхаю на даче", "Работаю на даче",
                            "Отдыхаю в санатории", "Отдыхаю в деревне",
                            "Езжу на море в России", "Езжу на море за границу",
                            "Путешествую по России", "Путешествую за границей",
                        ]),
                    },
                ],
            },
            new Poll
            {
                Id = CreateGuidV7(),
                Name = "Заработки",
                Html = null,
                IsActive = true,
                Questions = [
                    new Question{
                        Id = CreateGuidV7(),
                        Text = "Сколько вы зарабатываете в месяц?",
                        AllowCustomAnswer = false,
                        AllowMultipleChoice = false,
                        Number = 1,
                        TargetAnswer = null,
                        MatchNextNumber = null,
                        DefaultNextNumber = null,
                        Answers = CreateAnswers([
                            "Ничего не зарабатываю", "менее 5 тысяч рублей",
                            "от 5 до 10 т.р.", "от 10 до 15 т.р.", "от 15 до 20 т.р.",
                            "от 20 до 25 т.р.", "от 25 до 30 т.р.", "от 30 до 40 т.р.",
                            "от 40 до 50 т.р.", "от 50 до 60 т.р.", "от 60 до 70 т.р.",
                            "от 70 до 80 т.р.", "от 80 до 90 т.р.", "от 90 до 100 т.р.",
                            "от 100 до 120 т.р.", "от 120 до 140 т.р.", "от 140 до 160 т.р.",
                            "от 160 до 180 т.р.", "от 180 до 200 т.р.", "от 200 до 250 т.р.",
                            "от 250 до 300 т.р.", "от 300 до 350 т.р.", "от 350 до 400 т.р.",
                            "от 400 до 500 т.р.", "от 500 до 600 т.р.", "от 600 до 700 т.р.",
                            "от 700 до 800 т.р.", "от 800 до 1000 т.р.", "от 1 до 2 м.р.",
                            "от 2 до 3 м.р.", "от 3 до 4 м.р.", "от 4 до 5 м.р.",
                            "больше 5 миллионов рублей",
                        ]),
                    },
                ],
            },
            new Poll
            {
                Id = CreateGuidV7(),
                Name = "Работа",
                Html = null,
                IsActive = true,
                Questions = [
                    new Question{
                        Id = CreateGuidV7(),
                        Text = "Вы работаете?",
                        AllowCustomAnswer = false,
                        AllowMultipleChoice = false,
                        Number = 1,
                        TargetAnswer = null,
                        MatchNextNumber = null,
                        DefaultNextNumber = null,
                        Answers = CreateAnswers([
                            "Не работаю, учусь, не ищу работу",
                            "Не работаю, есть пассивный доход, не ищу работу",
                            "Не работаю, есть пассивный доход, ищу работу",
                            "Не работаю, нет доходов, не ищу работу",
                            "Не работаю, нет доходов, ищу работу",
                            "Работаю по 6 дней через 2 по 7 часов",
                            "Работаю по 5 дней через 2 по 8 часов",
                            "Работаю по 5 дней через 2 по 6 часов",
                            "Работаю по 5 дней через 2 по 4 часа",
                            "Работаю по 3 дня через 3 по 12 часов",
                            "Работаю по 3 дня через 3 по 11 часов",
                            "Работаю по 2 дня через 2 по 12 часов",
                            "Работаю по 2 дня через 2 по 11 часов",
                            "Работаю по 1 дню через 3 по 24 часа",
                            "Работаю на нескольких работах",
                        ]),
                    },
                ],
            },
            new Poll
            {
                Id = CreateGuidV7(),
                Name = "Места жительства",
                Html = null,
                IsActive = true,
                Questions = [
                    new Question{
                        Id = CreateGuidV7(),
                        Text = "Где живёте?",
                        AllowCustomAnswer = false,
                        AllowMultipleChoice = false,
                        Number = 1,
                        TargetAnswer = null,
                        MatchNextNumber = null,
                        DefaultNextNumber = null,
                        Answers = CreateAnswers([
                            "В Москве",
                            "В Питере",
                            "В городе-миллионнике",
                            "В городе от 500т.ж.",
                            "В городе от 200т.ж.",
                            "В городе от 100т.ж.",
                            "В городе от 50т.ж.",
                            "В городе от 20т.ж.",
                            "В городе от 10т.ж.",
                            "В селе",
                        ]),
                    },
                ],
            },
            new Poll
            {
                Id = CreateGuidV7(),
                Name = "Образование",
                Html = null,
                IsActive = true,
                Questions = [
                    new Question{
                        Id = CreateGuidV7(),
                        Text = "Какое у вас образование?",
                        AllowCustomAnswer = false,
                        AllowMultipleChoice = true,
                        Number = 1,
                        TargetAnswer = null,
                        MatchNextNumber = null,
                        DefaultNextNumber = null,
                        Answers = CreateAnswers([
                            "9 классов",
                            "11 классов",
                            "ПТУ",
                            "Колледж",
                            "Университет - бакалавр",
                            "Университет - специалитет",
                            "Университет - магистратура",
                            "Аспирантура",
                            "Кандидат наук",
                            "Доктор наук",
                            "Несколько высших",
                            "Несколько кандидатских",
                        ]),
                    },
                ],
            },
            new Poll
            {
                Id = CreateGuidV7(),
                Name = "Религия",
                Html = null,
                IsActive = true,
                Questions = [
                    new Question{
                        Id = CreateGuidV7(),
                        Text = "Какую религию вы исповедуете?",
                        AllowCustomAnswer = false,
                        AllowMultipleChoice = false,
                        Number = 1,
                        TargetAnswer = null,
                        MatchNextNumber = null,
                        DefaultNextNumber = null,
                        Answers = CreateAnswers([
                            "Атеист, не верю в сверхъестественное",
                            "Не верю в религии но верю в магию",
                            "Православный",
                            "Католик",
                            "Иудей",
                            "Индуист",
                            "Буддист",
                            "Дзен-буддист",
                            "Приверженец религий из фантастических книг/фильмов/игр",
                        ]),
                    },
                ],
            },
        ];

        return polls;
    }
}
