using System;
using System.Threading.Tasks;

using FluentAssertions;
using FluentAssertions.Execution;

using Ilnitsky.Polls.BusinessLogic;
using Ilnitsky.Polls.DataAccess.Entities.Answers;
using Ilnitsky.Polls.Middlewares;
using Ilnitsky.Polls.Tests.Shared;

using Microsoft.AspNetCore.Http;

using Moq;

namespace Ilnitsky.Polls.Tests.NUnit.Fluent.Middlewares;

public class RespondentMiddlewareTests
{
    [Test]
    public async Task InvokeAsync_CallsNextAndDoesNotModifyDb_WhenIdIsInSession()
    {
        // Arrange
        using var dbContext = ContextHelper.CreateDbContext();

        var httpContext = ContextHelper.CreateHttpContext(dbContext);
        var respondentIdString = GuidHelper.CreateGuidV7().ToString();
        httpContext.Session.SetString("RespondentId", respondentIdString);

        bool wasNextCalled = false;
        var middleware = new RespondentMiddleware((innerContext) =>
        {
            wasNextCalled = true;
            return Task.CompletedTask;
        });

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        using (new AssertionScope())
        {
            // Проверяем, что в БД ничего не добавилось
            dbContext.Respondents.Should().BeEmpty();
            // Проверяем что следующий Middleware был вызван
            wasNextCalled.Should().BeTrue();
        }
    }

    [Test]
    public async Task InvokeAsync_SavesInSession_WhenIdExistsInCookiesAndDb()
    {
        // Arrange
        using var dbContext = ContextHelper.CreateDbContext();
        var respondentId = GuidHelper.CreateGuidV7();
        dbContext.Respondents.Add(new Respondent { Id = respondentId });
        await dbContext.SaveChangesAsync();

        var httpContext = ContextHelper.CreateHttpContext(dbContext);
        httpContext.Request.Headers.Append("Cookie", $"RespondentId={respondentId}");

        bool wasNextCalled = false;
        var middleware = new RespondentMiddleware((innerContext) =>
        {
            wasNextCalled = true;
            return Task.CompletedTask;
        });

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        using (new AssertionScope())
        {
            httpContext.Session.GetString("RespondentId")
                .Should().Be(respondentId.ToString());
            // Проверяем, что кука обновилась в ответе
            httpContext.Response.Headers.SetCookie.ToString()
                .Should().Contain("RespondentId");
            // Убеждаемся, что запись как была одна, так и осталась
            dbContext.Respondents
                .Should().ContainSingle();
            wasNextCalled
                .Should().BeTrue();
        }
    }

    [Test]
    public async Task InvokeAsync_CreatesNewRespondent_WhenNoIdExists()
    {
        // Arrange
        using var dbContext = ContextHelper.CreateDbContext();

        var httpContext = ContextHelper.CreateHttpContext(dbContext);

        bool wasNextCalled = false;
        var middleware = new RespondentMiddleware((innerContext) =>
        {
            wasNextCalled = true;
            return Task.CompletedTask;
        });

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        using (new AssertionScope())
        {
            var createdIdString = httpContext.Session.GetString("RespondentId");

            createdIdString
                .Should().NotBeNull();
            Guid.TryParse(createdIdString, out _)
                .Should().BeTrue();
            // Проверяем наличие в базе
            dbContext.Respondents
                .Should().ContainSingle(r => r.Id == Guid.Parse(createdIdString));
            wasNextCalled
                .Should().BeTrue();
        }
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase("not-a-guid-value")]
    [TestCase("1234567890")]
    public async Task InvokeAsync_CreatesNewRespondent_WhenCookieIdIsInvalid(string badGuid)
    {
        // Arrange
        using var dbContext = ContextHelper.CreateDbContext();
        var httpContext = ContextHelper.CreateHttpContext(dbContext);

        // Подкладываем невалидный GUID в куки
        httpContext.Request.Headers.Append("Cookie", $"RespondentId={badGuid}");

        bool wasNextCalled = false;
        var middleware = new RespondentMiddleware((innerContext) =>
        {
            wasNextCalled = true;
            return Task.CompletedTask;
        });

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        using (new AssertionScope())
        {
            // Проверяем, что Middleware не упал, а создал новый валидный Id
            var sessionIdString = httpContext.Session.GetString("RespondentId");

            Guid.TryParse(sessionIdString, out var newGuid)
                .Should().BeTrue();
            dbContext.Respondents
                .Should().ContainSingle(r => r.Id == newGuid);
            httpContext.Response.Headers.SetCookie.ToString()
                .Should().Contain($"RespondentId={sessionIdString}");
            wasNextCalled
                .Should().BeTrue();
        }
    }
}
