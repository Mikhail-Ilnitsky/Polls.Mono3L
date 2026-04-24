using System;
using System.Threading.Tasks;

using Ilnitsky.Polls.BusinessLogic.Handlers.Answers;
using Ilnitsky.Polls.Contracts.Dtos;
using Ilnitsky.Polls.Contracts.Dtos.Answers;
using Ilnitsky.Polls.Controllers;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Moq;

namespace Ilnitsky.Polls.Tests.XUnit.Controllers;

public class AnswersControllerTests
{
    private readonly Mock<ICreateRespondentAnswerHandler> _handlerMock;
    private readonly Mock<ISession> _sessionMock;
    private readonly DefaultHttpContext _httpContext;
    private readonly AnswersController _controller;

    public AnswersControllerTests()
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
        Assert.True(_httpContext.Items.ContainsKey("ErrorDetails"));
        var errorDetails = _httpContext.Items["ErrorDetails"] as string;
        Assert.StartsWith(expectedStart, errorDetails);
        Assert.Contains(expectedValue, errorDetails);
    }

    [Fact]
    public async Task CreateRespondentAnswer_ReturnsBadRequest_WhenRespondentIdIsMissing()
    {
        // Arrange
        var dto = new CreateRespondentAnswerDto(PollId: Guid.NewGuid(), QuestionId: Guid.NewGuid(), Answers: ["Ответ"]);

        SetSessionValue("RespondentSessionId", Guid.NewGuid().ToString());

        // Act
        var actionResult = await _controller.CreateRespondentAnswer(dto, _handlerMock.Object);

        // Assert
        var badRequestObjectResult = Assert.IsType<BadRequestObjectResult>(actionResult);
        Assert.Equal("Некорректный идентификатор респондента!", badRequestObjectResult.Value);
        AssertErrorDetails("Некорректное значение respondentId =", "''");
    }

    [Fact]
    public async Task CreateRespondentAnswer_ReturnsBadRequest_WhenRespondentSessionIdIsMissing()
    {
        // Arrange
        var dto = new CreateRespondentAnswerDto(PollId: Guid.NewGuid(), QuestionId: Guid.NewGuid(), Answers: ["Ответ"]);

        SetSessionValue("RespondentId", Guid.NewGuid().ToString());

        // Act
        var actionResult = await _controller.CreateRespondentAnswer(dto, _handlerMock.Object);

        // Assert
        var badRequestObjectResult = Assert.IsType<BadRequestObjectResult>(actionResult);
        Assert.Equal("Некорректный идентификатор сессии респондента!", badRequestObjectResult.Value);
        AssertErrorDetails("Некорректное значение respondentSessionId =", "''");
    }

    [Fact]
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
        var badRequestObjectResult = Assert.IsType<BadRequestObjectResult>(actionResult);
        Assert.Equal("Некорректный идентификатор респондента!", badRequestObjectResult.Value);
        AssertErrorDetails("Некорректное значение respondentId =", incorrectGuidString);
    }

    [Fact]
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
        var badRequestObjectResult = Assert.IsType<BadRequestObjectResult>(actionResult);
        Assert.Equal("Некорректный идентификатор сессии респондента!", badRequestObjectResult.Value);
        AssertErrorDetails("Некорректное значение respondentSessionId =", incorrectGuidString);
    }

    [Fact]
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

        var createdResult = Assert.IsType<CreatedResult>(actionResult);
        Assert.Equal("Ответ принят!", createdResult.Value);
    }

    [Fact]
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
        var notFoundObjectResult = Assert.IsType<NotFoundObjectResult>(actionResult);
        var problemDetails = Assert.IsType<ProblemDetails>(notFoundObjectResult.Value);

        Assert.Equal(404, problemDetails.Status);
        Assert.Equal("Ошибка!", problemDetails.Title);
        Assert.Equal("Не найден опрос!", problemDetails.Detail);
        AssertErrorDetails("Не найден опрос!", pollId.ToString());
    }
}
