using System;
using System.Linq;
using System.Threading.Tasks;

using Ilnitsky.Polls.BusinessLogic;
using Ilnitsky.Polls.BusinessLogic.Handlers.Answers;
using Ilnitsky.Polls.Contracts.Dtos;
using Ilnitsky.Polls.Contracts.Dtos.Answers;
using Ilnitsky.Polls.DataAccess;
using Ilnitsky.Polls.DataAccess.Entities.Answers;
using Ilnitsky.Polls.Tests.Shared;

using Microsoft.EntityFrameworkCore;

namespace Ilnitsky.Polls.Tests.NUnit.Handlers;

public class CreateRespondentAnswerHandlerTests
{
    private ApplicationDbContext _dbContext;
    private CreateRespondentAnswerHandler _handler;

    [SetUp]
    public void Setup()
    {
        // Создаем уникальное имя БД для каждого запуска теста
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _handler = new CreateRespondentAnswerHandler(_dbContext);
    }

    [Test]
    public async Task HandleAsync_ReturnsIncorrectValue_WhenAnswersIsEmpty()
    {
        // Arrange
        var (poll, pollId, _) = TestDbHelper.CreatePoll();
        _dbContext.Polls.Add(poll);
        await _dbContext.SaveChangesAsync();

        var questionId = poll.Questions.First().Id;
        var answerDto = new CreateRespondentAnswerDto(pollId, questionId, []);

        // Act
        var result = await _handler.HandleAsync(answerDto, Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ErrorType, Is.EqualTo(ErrorType.IncorrectValue));
        Assert.That(result.Message, Is.EqualTo("Не задан ответ!"));
        Assert.That(result.ErrorDetails, Is.Not.Null);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    [TestCase("        ")]
    public async Task HandleAsync_ReturnsIncorrectValue_WhenAnswerStringIsNullOrWhiteSpace(string? invalidValue)
    {
        // Arrange
        var (poll, pollId, _) = TestDbHelper.CreatePoll();
        _dbContext.Polls.Add(poll);
        await _dbContext.SaveChangesAsync();

        var questionId = poll.Questions.First().Id;
        var answerDto = new CreateRespondentAnswerDto(pollId, questionId, [invalidValue!]);

        // Act
        var result = await _handler.HandleAsync(answerDto, Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ErrorType, Is.EqualTo(ErrorType.IncorrectValue));
        Assert.That(result.Message, Is.EqualTo("Не должно быть пустых ответов!"));
        Assert.That(result.ErrorDetails, Is.Not.Null);
    }

    [Test]
    public async Task HandleAsync_ReturnsEntityNotFound_WhenQuestionNotFound()
    {
        // Arrange
        var (poll, pollId, _) = TestDbHelper.CreatePoll();
        var respondentId = GuidHelper.CreateGuidV7();
        var respondentSessionId = GuidHelper.CreateGuidV7();
        _dbContext.Polls.Add(poll);
        _dbContext.Respondents.Add(new Respondent
        {
            Id = respondentId,
            DateTime = DateTime.UtcNow
        });
        _dbContext.RespondentSessions.Add(new RespondentSession
        {
            Id = respondentSessionId,
            RespondentId = respondentId,
            DateTime = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var badQuestionId = GuidHelper.CreateGuidV7();
        var answerDto = new CreateRespondentAnswerDto(pollId, badQuestionId, ["Ответ"]);

        // Act
        var result = await _handler.HandleAsync(answerDto, respondentSessionId, respondentId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ErrorType, Is.EqualTo(ErrorType.EntityNotFound));
        Assert.That(result.Message, Is.EqualTo("Не найден вопрос!"));
        Assert.That(result.ErrorDetails, Is.Not.Null);
        Assert.That(result.ErrorDetails, Does.Contain(badQuestionId.ToString()));
    }

    [Test]
    public async Task HandleAsync_ReturnsEntityNotFound_WhenRespondentNotFound()
    {
        // Arrange
        var (poll, pollId, _) = TestDbHelper.CreatePoll();
        var respondentId = GuidHelper.CreateGuidV7();
        var respondentSessionId = GuidHelper.CreateGuidV7();
        _dbContext.Polls.Add(poll);
        _dbContext.Respondents.Add(new Respondent
        {
            Id = respondentId,
            DateTime = DateTime.UtcNow
        });
        _dbContext.RespondentSessions.Add(new RespondentSession
        {
            Id = respondentSessionId,
            RespondentId = respondentId,
            DateTime = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var questionId = poll.Questions.First().Id;
        var answerText = poll.Questions.First().Answers.First().Text;
        var answerDto = new CreateRespondentAnswerDto(pollId, questionId, [answerText!]);

        var badRespondentId = GuidHelper.CreateGuidV7();

        // Act
        var result = await _handler.HandleAsync(answerDto, respondentSessionId, badRespondentId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ErrorType, Is.EqualTo(ErrorType.EntityNotFound));
        Assert.That(result.Message, Is.EqualTo("Не найден респондент!"));
        Assert.That(result.ErrorDetails, Is.Not.Null);
        Assert.That(result.ErrorDetails, Does.Contain(badRespondentId.ToString()));
    }

    [Test]
    public async Task HandleAsync_ReturnsEntityNotFound_WhenRespondentSessionNotFound()
    {
        // Arrange
        var (poll, pollId, _) = TestDbHelper.CreatePoll();
        var respondentId = GuidHelper.CreateGuidV7();
        var respondentSessionId = GuidHelper.CreateGuidV7();
        _dbContext.Polls.Add(poll);
        _dbContext.Respondents.Add(new Respondent
        {
            Id = respondentId,
            DateTime = DateTime.UtcNow
        });
        _dbContext.RespondentSessions.Add(new RespondentSession
        {
            Id = respondentSessionId,
            RespondentId = respondentId,
            DateTime = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var questionId = poll.Questions.First().Id;
        var answerText = poll.Questions.First().Answers.First().Text;
        var answerDto = new CreateRespondentAnswerDto(pollId, questionId, [answerText!]);

        var badRespondentSessionId = GuidHelper.CreateGuidV7();

        // Act
        var result = await _handler.HandleAsync(answerDto, badRespondentSessionId, respondentId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ErrorType, Is.EqualTo(ErrorType.EntityNotFound));
        Assert.That(result.Message, Is.EqualTo("Не найдена сессия респондента!"));
        Assert.That(result.ErrorDetails, Is.Not.Null);
        Assert.That(result.ErrorDetails, Does.Contain(badRespondentSessionId.ToString()));
    }

    [Test]
    public async Task HandleAsync_ReturnsEntityNotFound_WhenPollNotFound()
    {
        // Arrange
        var (poll, _, _) = TestDbHelper.CreatePoll();
        var respondentId = GuidHelper.CreateGuidV7();
        var respondentSessionId = GuidHelper.CreateGuidV7();
        _dbContext.Polls.Add(poll);
        _dbContext.Respondents.Add(new Respondent
        {
            Id = respondentId,
            DateTime = DateTime.UtcNow
        });
        _dbContext.RespondentSessions.Add(new RespondentSession
        {
            Id = respondentSessionId,
            RespondentId = respondentId,
            DateTime = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var questionId = poll.Questions.First().Id;
        var answerText = poll.Questions.First().Answers.First().Text;
        var badPollId = GuidHelper.CreateGuidV7();
        var answerDto = new CreateRespondentAnswerDto(badPollId, questionId, [answerText!]);

        // Act
        var result = await _handler.HandleAsync(answerDto, respondentSessionId, respondentId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ErrorType, Is.EqualTo(ErrorType.EntityNotFound));
        Assert.That(result.Message, Is.EqualTo("Не найден опрос!"));
        Assert.That(result.ErrorDetails, Is.Not.Null);
        Assert.That(result.ErrorDetails, Does.Contain(badPollId.ToString()));
    }

    [Test]
    public async Task HandleAsync_ReturnsIncorrectValue_For2IdenticalAnswers_WhenAllowMultipleChoice()
    {
        // Arrange
        var (poll, pollId, _) = TestDbHelper.CreatePoll();
        var respondentId = GuidHelper.CreateGuidV7();
        var respondentSessionId = GuidHelper.CreateGuidV7();

        poll.Questions.First().AllowMultipleChoice = true;
        poll.Questions.First().AllowCustomAnswer = false;

        _dbContext.Polls.Add(poll);
        _dbContext.Respondents.Add(new Respondent
        {
            Id = respondentId,
            DateTime = DateTime.UtcNow
        });
        _dbContext.RespondentSessions.Add(new RespondentSession
        {
            Id = respondentSessionId,
            RespondentId = respondentId,
            DateTime = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var questionId = poll.Questions.First().Id;
        var answerText = poll.Questions.First().Answers.First().Text;
        var answerDto = new CreateRespondentAnswerDto(pollId, questionId, [answerText!, answerText!]);

        // Act
        var result = await _handler.HandleAsync(answerDto, respondentSessionId, respondentId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ErrorType, Is.EqualTo(ErrorType.IncorrectValue));
        Assert.That(result.Message, Is.EqualTo("Не должно быть одинаковых ответов!"));
        Assert.That(result.ErrorDetails, Is.Not.Null);
    }

    [Test]
    public async Task HandleAsync_ReturnsIncorrectValue_ForMultipleAnswers_WhenNotAllowMultipleChoice()
    {
        // Arrange
        var (poll, pollId, _) = TestDbHelper.CreatePoll();
        var respondentId = GuidHelper.CreateGuidV7();
        var respondentSessionId = GuidHelper.CreateGuidV7();

        poll.Questions.First().AllowMultipleChoice = false;
        poll.Questions.First().AllowCustomAnswer = false;

        _dbContext.Polls.Add(poll);
        _dbContext.Respondents.Add(new Respondent
        {
            Id = respondentId,
            DateTime = DateTime.UtcNow
        });
        _dbContext.RespondentSessions.Add(new RespondentSession
        {
            Id = respondentSessionId,
            RespondentId = respondentId,
            DateTime = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var questionId = poll.Questions.First().Id;
        var answer1Text = poll.Questions.First().Answers.First().Text;
        var answer2Text = poll.Questions.First().Answers.Last().Text;
        var answerDto = new CreateRespondentAnswerDto(pollId, questionId, [answer1Text!, answer2Text!]);

        // Act
        var result = await _handler.HandleAsync(answerDto, respondentSessionId, respondentId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ErrorType, Is.EqualTo(ErrorType.IncorrectValue));
        Assert.That(result.Message, Is.EqualTo("На этот вопрос не должно быть больше одного ответа!"));
        Assert.That(result.ErrorDetails, Is.Not.Null);
    }

    [Test]
    public async Task HandleAsync_ReturnsIncorrectValue_ForCustomAnswer_WhenNotAllowCustomAnswer()
    {
        // Arrange
        var (poll, pollId, _) = TestDbHelper.CreatePoll();
        var respondentId = GuidHelper.CreateGuidV7();
        var respondentSessionId = GuidHelper.CreateGuidV7();

        poll.Questions.First().AllowMultipleChoice = false;
        poll.Questions.First().AllowCustomAnswer = false;

        _dbContext.Polls.Add(poll);
        _dbContext.Respondents.Add(new Respondent
        {
            Id = respondentId,
            DateTime = DateTime.UtcNow
        });
        _dbContext.RespondentSessions.Add(new RespondentSession
        {
            Id = respondentSessionId,
            RespondentId = respondentId,
            DateTime = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var questionId = poll.Questions.First().Id;
        var answerDto = new CreateRespondentAnswerDto(pollId, questionId, ["произвольный ответ"]);

        // Act
        var result = await _handler.HandleAsync(answerDto, respondentSessionId, respondentId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ErrorType, Is.EqualTo(ErrorType.IncorrectValue));
        Assert.That(result.Message, Is.EqualTo("На этот вопрос не должно быть произвольного ответа!"));
        Assert.That(result.ErrorDetails, Is.Not.Null);
    }

    [Test]
    public async Task HandleAsync_ReturnsCreated_ForSingleRegularAnswer()
    {
        // Arrange
        var (poll, pollId, _) = TestDbHelper.CreatePoll();
        var respondentId = GuidHelper.CreateGuidV7();
        var respondentSessionId = GuidHelper.CreateGuidV7();

        poll.Questions.First().AllowMultipleChoice = false;
        poll.Questions.First().AllowCustomAnswer = false;

        _dbContext.Polls.Add(poll);
        _dbContext.Respondents.Add(new Respondent
        {
            Id = respondentId,
            DateTime = DateTime.UtcNow
        });
        _dbContext.RespondentSessions.Add(new RespondentSession
        {
            Id = respondentSessionId,
            RespondentId = respondentId,
            DateTime = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var questionId = poll.Questions.First().Id;
        var answerText = poll.Questions.First().Answers.First().Text;
        var answerDto = new CreateRespondentAnswerDto(pollId, questionId, [answerText!]);

        // Act
        var result = await _handler.HandleAsync(answerDto, respondentSessionId, respondentId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.ErrorType, Is.EqualTo(ErrorType.None));
        Assert.That(result.ErrorDetails, Is.Null);
    }

    [Test]
    public async Task HandleAsync_ReturnsCreated_ForSingleRegularAnswer_WhenAllowMultipleChoice()
    {
        // Arrange
        var (poll, pollId, _) = TestDbHelper.CreatePoll();
        var respondentId = GuidHelper.CreateGuidV7();
        var respondentSessionId = GuidHelper.CreateGuidV7();

        poll.Questions.First().AllowMultipleChoice = true;
        poll.Questions.First().AllowCustomAnswer = false;

        _dbContext.Polls.Add(poll);
        _dbContext.Respondents.Add(new Respondent
        {
            Id = respondentId,
            DateTime = DateTime.UtcNow
        });
        _dbContext.RespondentSessions.Add(new RespondentSession
        {
            Id = respondentSessionId,
            RespondentId = respondentId,
            DateTime = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var questionId = poll.Questions.First().Id;
        var answerText = poll.Questions.First().Answers.First().Text;
        var answerDto = new CreateRespondentAnswerDto(pollId, questionId, [answerText!]);

        // Act
        var result = await _handler.HandleAsync(answerDto, respondentSessionId, respondentId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.ErrorType, Is.EqualTo(ErrorType.None));
        Assert.That(result.ErrorDetails, Is.Null);
    }

    [Test]
    public async Task HandleAsync_ReturnsCreated_ForSingleRegularAnswer_WhenAllowCustomAnswer()
    {
        // Arrange
        var (poll, pollId, _) = TestDbHelper.CreatePoll();
        var respondentId = GuidHelper.CreateGuidV7();
        var respondentSessionId = GuidHelper.CreateGuidV7();

        poll.Questions.First().AllowMultipleChoice = false;
        poll.Questions.First().AllowCustomAnswer = true;

        _dbContext.Polls.Add(poll);
        _dbContext.Respondents.Add(new Respondent
        {
            Id = respondentId,
            DateTime = DateTime.UtcNow
        });
        _dbContext.RespondentSessions.Add(new RespondentSession
        {
            Id = respondentSessionId,
            RespondentId = respondentId,
            DateTime = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var questionId = poll.Questions.First().Id;
        var answerText = poll.Questions.First().Answers.First().Text;
        var answerDto = new CreateRespondentAnswerDto(pollId, questionId, [answerText!]);

        // Act
        var result = await _handler.HandleAsync(answerDto, respondentSessionId, respondentId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.ErrorType, Is.EqualTo(ErrorType.None));
        Assert.That(result.ErrorDetails, Is.Null);
    }

    [Test]
    public async Task HandleAsync_ReturnsCreated_For2Answers_WhenAllowMultipleChoice()
    {
        // Arrange
        var (poll, pollId, _) = TestDbHelper.CreatePoll();
        var respondentId = GuidHelper.CreateGuidV7();
        var respondentSessionId = GuidHelper.CreateGuidV7();

        poll.Questions.First().AllowMultipleChoice = true;
        poll.Questions.First().AllowCustomAnswer = false;

        _dbContext.Polls.Add(poll);
        _dbContext.Respondents.Add(new Respondent
        {
            Id = respondentId,
            DateTime = DateTime.UtcNow
        });
        _dbContext.RespondentSessions.Add(new RespondentSession
        {
            Id = respondentSessionId,
            RespondentId = respondentId,
            DateTime = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var questionId = poll.Questions.First().Id;
        var answer1Text = poll.Questions.First().Answers.First().Text;
        var answer2Text = poll.Questions.First().Answers.Last().Text;
        var answerDto = new CreateRespondentAnswerDto(pollId, questionId, [answer1Text!, answer2Text!]);

        // Act
        var result = await _handler.HandleAsync(answerDto, respondentSessionId, respondentId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.ErrorType, Is.EqualTo(ErrorType.None));
        Assert.That(result.ErrorDetails, Is.Null);
    }

    [Test]
    public async Task HandleAsync_ReturnsCreated_ForCustomAnswer_WhenAllowCustomAnswer()
    {
        // Arrange
        var (poll, pollId, _) = TestDbHelper.CreatePoll();
        var respondentId = GuidHelper.CreateGuidV7();
        var respondentSessionId = GuidHelper.CreateGuidV7();

        poll.Questions.First().AllowMultipleChoice = false;
        poll.Questions.First().AllowCustomAnswer = true;

        _dbContext.Polls.Add(poll);
        _dbContext.Respondents.Add(new Respondent
        {
            Id = respondentId,
            DateTime = DateTime.UtcNow
        });
        _dbContext.RespondentSessions.Add(new RespondentSession
        {
            Id = respondentSessionId,
            RespondentId = respondentId,
            DateTime = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var questionId = poll.Questions.First().Id;
        var answerDto = new CreateRespondentAnswerDto(pollId, questionId, ["произвольный ответ"]);

        // Act
        var result = await _handler.HandleAsync(answerDto, respondentSessionId, respondentId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.ErrorType, Is.EqualTo(ErrorType.None));
        Assert.That(result.ErrorDetails, Is.Null);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
    }
}
