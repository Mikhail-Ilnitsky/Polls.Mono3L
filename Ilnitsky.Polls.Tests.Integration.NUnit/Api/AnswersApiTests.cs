using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using Ilnitsky.Polls.Contracts.Dtos.Answers;
using Ilnitsky.Polls.DataAccess;
using Ilnitsky.Polls.DataAccess.Entities.Answers;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ilnitsky.Polls.Tests.Integration.NUnit.Api;

public class AnswersApiTests
{
    [Test]
    public async Task CreateRespondentAnswer_SavesToDatabase_WhenAllowCustomAnswer()
    {
        // Arrange
        // Используем клиент, который умеет хранить куки сессии
        using var httpClient = GlobalTestsSetup.Factory.GetHttpClientWithCookies();

        // Находим реальный опрос с одним вопросом позволяющим произвольные ответы
        using var scope = GlobalTestsSetup.Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var poll = await dbContext.Polls
            .Include(p => p.Questions)
                .ThenInclude(q => q.Answers)
            .Where(p => p.Questions.Count == 1
                && p.Questions.First().AllowCustomAnswer
                && !p.Questions.First().AllowMultipleChoice)
            .FirstAsync();
        var questionId = poll.Questions.First().Id;
        var customAnswer = "Ответ #" + Guid.NewGuid().ToString();

        var answerDto = new CreateRespondentAnswerDto(
            PollId: poll.Id,
            QuestionId: questionId,
            Answers: [customAnswer]
        );

        // Act
        // Отправляем ответ. Клиент автоматически приложит куку сессии
        var response = await httpClient.PostAsJsonAsync("api/v1/answers", answerDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Ответ принят!");

        // Проверяем, что в MariaDB реально появилась запись
        var savedAnswer = await dbContext.RespondentAnswers
            .FirstOrDefaultAsync(a => a.QuestionId == questionId && a.Text == customAnswer);

        savedAnswer.Should().NotBeNull();
        savedAnswer.RespondentId.Should().NotBe(Guid.Empty);
        savedAnswer.RespondentSessionId.Should().NotBe(Guid.Empty);
    }

    [Test]
    public async Task CreateRespondentAnswer_SavesToDatabase_WhenNotAllowCustomAnswer()
    {
        // Arrange
        // Используем клиент, который умеет хранить куки сессии
        using var httpClient = GlobalTestsSetup.Factory.GetHttpClientWithCookies();

        // Находим реальный опрос с одним вопросом не позволяющим произвольные ответы
        using var scope = GlobalTestsSetup.Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var poll = await dbContext.Polls
            .Include(p => p.Questions)
                .ThenInclude(q => q.Answers)
            .Where(p => p.Questions.Count == 1
                && !p.Questions.First().AllowCustomAnswer
                && !p.Questions.First().AllowMultipleChoice)
            .FirstAsync();
        var question = poll.Questions.First();
        var answer = question.Answers.First();

        var answerDto = new CreateRespondentAnswerDto(
            PollId: poll.Id,
            QuestionId: question.Id,
            Answers: [answer.Text]
        );

        // Act
        // Отправляем ответ. Клиент автоматически приложит куку сессии
        var response = await httpClient.PostAsJsonAsync("api/v1/answers", answerDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Ответ принят!");

        // Проверяем, что в MariaDB реально появилась запись
        var savedAnswer = await dbContext.RespondentAnswers
            .FirstOrDefaultAsync(a => a.QuestionId == question.Id && a.Text == answer.Text);

        savedAnswer.Should().NotBeNull();
        savedAnswer.RespondentId.Should().NotBe(Guid.Empty);
        savedAnswer.RespondentSessionId.Should().NotBe(Guid.Empty);
    }

    [TestCase(0, 1)]
    [TestCase(1, 2)]
    [TestCase(3, 3)]
    [TestCase(6, 4)]
    public async Task CreateRespondentAnswer_SavesToDatabase_WhenAllowMultipleChoice(int skipedAnswersCount, int answersCount)
    {
        // Arrange
        // Используем клиент, который умеет хранить куки сессии
        using var httpClient = GlobalTestsSetup.Factory.GetHttpClientWithCookies();

        // Находим реальный опрос с одним вопросом позволяющим выбрать несколько ответов
        using var scope = GlobalTestsSetup.Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var poll = await dbContext.Polls
            .Include(p => p.Questions)
                .ThenInclude(q => q.Answers)
            .Where(p => p.Questions.Count == 1
                && !p.Questions.First().AllowCustomAnswer
                && p.Questions.First().AllowMultipleChoice
                && p.Questions.First().Answers.Count >= 10)
            .FirstAsync();
        var question = poll.Questions.First();
        var answers = question.Answers
            .Skip(skipedAnswersCount)
            .Take(answersCount)
            .Select(a => a.Text ?? throw new Exception("Нет текста ответа"))
            .ToList();

        var answerDto = new CreateRespondentAnswerDto(
            PollId: poll.Id,
            QuestionId: question.Id,
            Answers: answers
        );

        // Act
        // Отправляем ответ. Клиент автоматически приложит куку сессии
        var response = await httpClient.PostAsJsonAsync("api/v1/answers", answerDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Ответ принят!");

        // Проверяем, что в MariaDB реально появилась запись
        var savedAnswers = await dbContext.RespondentAnswers
            .Where(a => a.QuestionId == question.Id && answers.Contains(a.Text!))
            .ToListAsync();
        savedAnswers.Should().HaveCount(answersCount);
    }

    [Test]
    public async Task Middleware_ShouldNotRecreate_RespondentId_IfItExists()
    {
        // Arrange
        using var scope = GlobalTestsSetup.Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var poll = await dbContext.Polls.FirstAsync();

        var container = new CookieContainer();
        using var httpClient = GlobalTestsSetup.Factory.GetHttpClientWithCookieContainer(container);

        // Act
        // 1. Первый вызов. Обязательно через https чтобы кука отправлялась!
        await httpClient.GetAsync($"https://localhost/api/v1/polls/{poll.Id}");
        var firstId = container.GetAllCookies()["RespondentId"]?.Value;

        // 2. Второй вызов. Обязательно через https чтобы кука отправлялась!
        await httpClient.GetAsync($"https://localhost/api/v1/polls/{poll.Id}");
        var secondId = container.GetAllCookies()["RespondentId"]?.Value;

        // Проверяем, умеет ли Middleware просто читать куку обратно
        secondId.Should().Be(firstId, "Middleware должно прочитать RespondentId из куки, а не создавать новый");
    }

    [TestCase("019c1aa8-9bf0-750d-1111-832de94b1c13")]
    [TestCase("019c1aa8-9bf0-750d-2222-832de94b1c13")]
    public async Task RespondentId_ArePersistent_FromCookie(Guid respondentId)
    {
        // Arrange
        using var scope = GlobalTestsSetup.Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Создаем тестового респондента в базе данных, 
        // так как мидлварь проверяет его наличие через dbContext.Respondents.Any()
        dbContext.Respondents.Add(new Respondent
        {
            Id = respondentId,
            DateTime = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync();

        // Инициализируем контейнер и клиента
        var cookieContainer = new CookieContainer();
        using var httpClient = GlobalTestsSetup.Factory.GetHttpClientWithCookieContainer(cookieContainer);

        // Используем HTTPS базовый адрес тестового сервера
        var baseUri = new Uri("https://localhost/");
        httpClient.BaseAddress = baseUri;

        // Вручную подкладываем куку в контейнер до первого запроса
        var customCookie = new Cookie("RespondentId", respondentId.ToString())
        {
            Domain = baseUri.Host,
            Path = "/",
            Secure = true, // Обязательно true, так как в мидлвари Secure = true
            HttpOnly = true
        };
        cookieContainer.Add(baseUri, customCookie);

        var poll = await dbContext.Polls
            .Include(p => p.Questions)
                .ThenInclude(q => q.Answers)
            .Where(p => p.Questions.Count == 1
                && p.Questions.First().AllowCustomAnswer)
            .FirstAsync();
        var questionId = poll.Questions.First().Id;
        var customAnswer = "Произвольный ответ #" + Guid.NewGuid().ToString();
        var answerDto = new CreateRespondentAnswerDto(
            PollId: poll.Id,
            QuestionId: questionId,
            Answers: [customAnswer]
        );

        // Act
        // Запрос сохраняет RespondentId полученный из куки
        await httpClient.PostAsJsonAsync("https://localhost/api/v1/answers", answerDto);

        // Assert
        // Проверяем в базе, что RespondentId соответствует этой сессии
        var respondentAnswer = await dbContext.RespondentAnswers
            .FirstOrDefaultAsync(a => a.Text == customAnswer);

        respondentAnswer.Should().NotBeNull();
        respondentAnswer.PollId.Should().Be(poll.Id);
        respondentAnswer.QuestionId.Should().Be(questionId);
        respondentAnswer.RespondentId.Should().Be(respondentId);
        respondentAnswer.Text.Should().Be(customAnswer);
    }

    [Test]
    public async Task RespondentId_ArePersistent_BetweenCalls()
    {
        // Arrange
        using var scope = GlobalTestsSetup.Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var poll = await dbContext.Polls
            .Include(p => p.Questions)
                .ThenInclude(q => q.Answers)
            .Where(p => p.Questions.Count == 1
                && p.Questions.First().AllowCustomAnswer)
            .FirstAsync();

        var container = new CookieContainer();
        using var httpClient = GlobalTestsSetup.Factory.GetHttpClientWithCookieContainer(container);

        // Act & Assert
        // Первый запрос (создаст сессию и RespondentId). Обязательно через https чтобы кука отправлялась!
        var response = await httpClient.GetAsync($"https://localhost/api/v1/polls/{poll.Id}");

        var cookies = container.GetAllCookies();
        var respondentIdCookie = cookies["RespondentId"];

        respondentIdCookie.Should().NotBeNull();
        respondentIdCookie.Value.Should().NotBeNull();

        var isCorrectRespondentId = Guid.TryParse(respondentIdCookie.Value, out var respondentId);
        isCorrectRespondentId.Should().BeTrue();

        var questionId = poll.Questions.First().Id;
        var customAnswer = "Произвольный ответ #" + Guid.NewGuid().ToString();
        var answerDto = new CreateRespondentAnswerDto(
            PollId: poll.Id,
            QuestionId: questionId,
            Answers: [customAnswer]
        );

        // Второй запрос (с той же кукой). Обязательно через https чтобы кука отправлялась!
        await httpClient.PostAsJsonAsync("https://localhost/api/v1/answers", answerDto);

        // Проверяем в базе, что RespondentId соответствует этой сессии
        var respondentAnswer = await dbContext.RespondentAnswers
            .FirstOrDefaultAsync(a => a.Text == customAnswer);

        respondentAnswer.Should().NotBeNull();
        respondentAnswer.PollId.Should().Be(poll.Id);
        respondentAnswer.QuestionId.Should().Be(questionId);
        respondentAnswer.RespondentId.Should().Be(respondentId);
        respondentAnswer.Text.Should().Be(customAnswer);
    }

    [Test]
    public async Task RespondentSessionId_ArePersistent_BetweenCalls()
    {
        // Arrange
        using var scope = GlobalTestsSetup.Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var poll = await dbContext.Polls
            .Include(p => p.Questions)
                .ThenInclude(q => q.Answers)
            .Where(p => p.Questions.Count > 1)
            .FirstAsync();

        var container = new CookieContainer();
        using var httpClient = GlobalTestsSetup.Factory.GetHttpClientWithCookieContainer(container);

        // Act & Assert
        // Первый запрос (создаст сессию и RespondentId). Обязательно через https чтобы кука отправлялась!
        var response = await httpClient.GetAsync($"https://localhost/api/v1/polls/{poll.Id}");

        var cookies = container.GetAllCookies();
        var respondentIdCookie = cookies["RespondentId"];

        respondentIdCookie.Should().NotBeNull();
        respondentIdCookie.Value.Should().NotBeNull();

        var isCorrectRespondentId = Guid.TryParse(respondentIdCookie.Value, out var respondentId);
        isCorrectRespondentId.Should().BeTrue();

        var question1 = poll.Questions.First();
        var answer1 = question1.Answers.Last().Text;
        var answer1Dto = new CreateRespondentAnswerDto(
            PollId: poll.Id,
            QuestionId: question1.Id,
            Answers: [answer1]
        );

        // Первый ответ (с той же кукой). Обязательно через https чтобы кука отправлялась!
        await httpClient.PostAsJsonAsync("https://localhost/api/v1/answers", answer1Dto);

        var respondentAnswer1 = await dbContext.RespondentAnswers
            .FirstOrDefaultAsync(a => a.Text == answer1 && a.QuestionId == question1.Id);

        respondentAnswer1.Should().NotBeNull();
        respondentAnswer1.PollId.Should().Be(poll.Id);
        respondentAnswer1.QuestionId.Should().Be(question1.Id);
        respondentAnswer1.RespondentId.Should().Be(respondentId);

        var question2 = poll.Questions.Skip(1).First();
        var answer2 = question2.Answers.Last().Text;
        var answer2Dto = new CreateRespondentAnswerDto(
            PollId: poll.Id,
            QuestionId: question2.Id,
            Answers: [answer2]
        );

        // Второй ответ (с той же кукой). Обязательно через https чтобы кука отправлялась!
        await httpClient.PostAsJsonAsync("https://localhost/api/v1/answers", answer2Dto);

        var respondentAnswer2 = await dbContext.RespondentAnswers
            .FirstOrDefaultAsync(a => a.Text == answer2 && a.QuestionId == question2.Id);

        respondentAnswer2.Should().NotBeNull();
        respondentAnswer2.PollId.Should().Be(poll.Id);
        respondentAnswer2.QuestionId.Should().Be(question2.Id);
        respondentAnswer2.RespondentId.Should().Be(respondentId);

        // Проверяем что сессия не изменилась
        respondentAnswer1.RespondentSessionId.Should().Be(respondentAnswer2.RespondentSessionId);
    }
}
