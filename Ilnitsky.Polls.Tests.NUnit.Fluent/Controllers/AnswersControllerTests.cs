using System;
using System.Threading.Tasks;

using FluentAssertions;

using Ilnitsky.Polls.BusinessLogic.Handlers.Answers;
using Ilnitsky.Polls.Contracts.Dtos;
using Ilnitsky.Polls.Contracts.Dtos.Answers;
using Ilnitsky.Polls.Controllers;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Moq;

namespace Ilnitsky.Polls.Tests.NUnit.Fluent.Controllers;

public class AnswersControllerTests
{
    private Mock<ICreateRespondentAnswerHandler> _handlerMock;
    private Mock<ISession> _sessionMock;
    private DefaultHttpContext _httpContext;
    private AnswersController _controller;

    [SetUp]
    public void Setup()
    {
        _handlerMock = new Mock<ICreateRespondentAnswerHandler>();
        _httpContext = new DefaultHttpContext();

        _sessionMock = new Mock<ISession>();
        _httpContext.Session = _sessionMock.Object;

        _controller = new AnswersController
        {
            ControllerContext = new ControllerContext { HttpContext = _httpContext }
        };
    }

    private void SetSessionValue(string key, string value)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(value);

        // Мокаем низкоуровневый TryGetValue, который используется внутри GetString
        _sessionMock
            .Setup(s => s.TryGetValue(key, out bytes))
            .Returns(true);
    }

    private void AssertErrorDetails(string expectedStart, string expectedValue)
    {
        _httpContext.Items
            .Should().ContainKey("ErrorDetails")
                .WhoseValue.Should().BeOfType<string>()
                    .Which.Should().StartWith(expectedStart)
                    .And.Contain(expectedValue);
    }

    [Test]
    public async Task CreateRespondentAnswer_ReturnsBadRequest_WhenRespondentIdIsMissing()
    {
        // Arrange
        var dto = new CreateRespondentAnswerDto(PollId: Guid.NewGuid(), QuestionId: Guid.NewGuid(), Answers: ["Ответ"]);

        SetSessionValue("RespondentSessionId", Guid.NewGuid().ToString());

        // Act
        var actionResult = await _controller.CreateRespondentAnswer(dto, _handlerMock.Object);

        // Assert
        actionResult
            .Should().BeOfType<BadRequestObjectResult>()
            .Which
                .Value.Should().Be("Некорректный идентификатор респондента!");

        AssertErrorDetails("Некорректное значение respondentId =", "''");
    }

    [Test]
    public async Task CreateRespondentAnswer_ReturnsBadRequest_WhenRespondentSessionIdIsMissing()
    {
        // Arrange
        var dto = new CreateRespondentAnswerDto(PollId: Guid.NewGuid(), QuestionId: Guid.NewGuid(), Answers: ["Ответ"]);

        SetSessionValue("RespondentId", Guid.NewGuid().ToString());

        // Act
        var actionResult = await _controller.CreateRespondentAnswer(dto, _handlerMock.Object);

        // Assert
        actionResult
            .Should().BeOfType<BadRequestObjectResult>()
            .Which
                .Value.Should().Be("Некорректный идентификатор сессии респондента!");

        AssertErrorDetails("Некорректное значение respondentSessionId =", "''");
    }

    [Test]
    public async Task CreateRespondentAnswer_ReturnsBadRequest_WhenRespondentIdIsIncorrect()
    {
        // Arrange
        var dto = new CreateRespondentAnswerDto(PollId: Guid.NewGuid(), QuestionId: Guid.NewGuid(), Answers: ["Ответ"]);
        var incorrectGuidString = "fsdfgsdg";

        SetSessionValue("RespondentId", incorrectGuidString);
        SetSessionValue("RespondentSessionId", Guid.NewGuid().ToString());

        // Act
        var actionResult = await _controller.CreateRespondentAnswer(dto, _handlerMock.Object);

        // Assert
        actionResult
            .Should().BeOfType<BadRequestObjectResult>()
            .Which
                .Value.Should().Be("Некорректный идентификатор респондента!");

        AssertErrorDetails("Некорректное значение respondentId =", incorrectGuidString);
    }

    [Test]
    public async Task CreateRespondentAnswer_ReturnsBadRequest_WhenRespondentSessionIdIsIncorrect()
    {
        // Arrange
        var dto = new CreateRespondentAnswerDto(PollId: Guid.NewGuid(), QuestionId: Guid.NewGuid(), Answers: ["Ответ"]);
        var incorrectGuidString = "fsdfgsdg";

        SetSessionValue("RespondentId", Guid.NewGuid().ToString());
        SetSessionValue("RespondentSessionId", incorrectGuidString);

        // Act
        var actionResult = await _controller.CreateRespondentAnswer(dto, _handlerMock.Object);

        // Assert
        actionResult
            .Should().BeOfType<BadRequestObjectResult>()
            .Which
                .Value.Should().Be("Некорректный идентификатор сессии респондента!");

        AssertErrorDetails("Некорректное значение respondentSessionId =", incorrectGuidString);
    }

    [Test]
    public async Task CreateRespondentAnswer_CallsHandler_WhenRespondentIdAndSessionAreValid()
    {
        // Arrange
        var respondentId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var dto = new CreateRespondentAnswerDto(PollId: Guid.NewGuid(), QuestionId: Guid.NewGuid(), Answers: ["Ответ"]);

        SetSessionValue("RespondentId", respondentId.ToString());
        SetSessionValue("RespondentSessionId", sessionId.ToString());

        var baseResponse = new BaseResponse(
            IsSuccess: true,
            IsCreated: true,
            Message: "Ответ принят!",
            ErrorDetails: null,
            ErrorType: ErrorType.None);

        _handlerMock
            .Setup(h => h.HandleAsync(dto, sessionId, respondentId))
            .ReturnsAsync(baseResponse);

        // Act
        var actionResult = await _controller.CreateRespondentAnswer(dto, _handlerMock.Object);

        // Assert
        _handlerMock.Verify(
            h => h.HandleAsync(dto, sessionId, respondentId),
            Times.Once);

        actionResult
            .Should().BeOfType<CreatedResult>()
            .Which
                .Value.Should().Be("Ответ принят!");
    }

    [Test]
    public async Task CreateRespondentAnswer_ReturnsNotFound_WhenHandlerReturnsEntityNotFound()
    {
        // Arrange
        var respondentId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var pollId = Guid.NewGuid();
        var dto = new CreateRespondentAnswerDto(pollId, Guid.NewGuid(), ["Ответ"]);

        SetSessionValue("RespondentId", respondentId.ToString());
        SetSessionValue("RespondentSessionId", sessionId.ToString());

        var errorBaseResponse = new BaseResponse(
            IsSuccess: false,
            IsCreated: false,
            Message: "Не найден опрос!",
            ErrorDetails: $"Нет опроса с Id = {pollId}",
            ErrorType: ErrorType.EntityNotFound);

        _handlerMock
            .Setup(h => h.HandleAsync(dto, sessionId, respondentId))
            .ReturnsAsync(errorBaseResponse);

        // Act
        var actionResult = await _controller.CreateRespondentAnswer(dto, _handlerMock.Object);

        // Assert
        actionResult
            .Should().BeOfType<NotFoundObjectResult>()
            .Which
                .Value.Should().BeOfType<ProblemDetails>()
                .Which.Should().BeEquivalentTo(
                new
                {
                    Status = 404,
                    Title = "Ошибка!",
                    Detail = "Не найден опрос!"
                });

        AssertErrorDetails("Не найден опрос!", pollId.ToString());
    }
}
