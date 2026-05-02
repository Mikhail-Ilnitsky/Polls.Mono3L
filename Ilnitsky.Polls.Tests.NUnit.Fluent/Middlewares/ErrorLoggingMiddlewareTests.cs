using System;
using System.Net;
using System.Threading.Tasks;

using FluentAssertions;
using FluentAssertions.Execution;

using Ilnitsky.Polls.Middlewares;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using Moq;

namespace Ilnitsky.Polls.Tests.NUnit.Fluent.Middlewares;

public class ErrorLoggingMiddlewareTests
{
    private Mock<ILogger<ErrorLoggingMiddleware>> _loggerMock;
    private DefaultHttpContext _httpContext;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<ErrorLoggingMiddleware>>();
        _httpContext = new DefaultHttpContext();
    }

    [Test]
    public async Task InvokeAsync_LogsErrorAndReturns500_WhenExceptionIsThrown()
    {
        // Arrange
        var middleware = new ErrorLoggingMiddleware(
            next: (innerContext) => throw new Exception("Тестовое исключение"),
            logger: _loggerMock.Object
        );

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        using (new AssertionScope())
        {
            _httpContext.Response.StatusCode
                .Should().Be((int)HttpStatusCode.InternalServerError);

            // Проверка, что LogError был вызван
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        static (v, t) =>
                            v != null
                            && v.ToString()!.StartsWith("Исключение для")
                            && v.ToString()!.Contains("Тестовое исключение")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    [TestCase("ErrorDetails")]
    [TestCase("ModelErrors")]
    [TestCase("BadResult")]
    public async Task InvokeAsync_LogsWarning_WhenErrorItemExists(string errorItemKey)
    {
        // Arrange
        const string errorMessage = "Тестовая ошибка";
        _httpContext.Items[errorItemKey] = errorMessage;

        var middleware = new ErrorLoggingMiddleware(
            next: (innerContext) => Task.CompletedTask,
            logger: _loggerMock.Object
        );

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(
                    static (v, t) =>
                        v != null
                        && v.ToString()!.StartsWith("Ошибка")
                        && v.ToString()!.Contains(errorMessage)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public async Task InvokeAsync_DoesNotLog_WhenNoErrorsExist()
    {
        // Arrange
        var middleware = new ErrorLoggingMiddleware(
            next: (innerContext) => Task.CompletedTask,
            logger: _loggerMock.Object
        );

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        // Проверяем, что логов НЕ БЫЛО
        _loggerMock.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}
