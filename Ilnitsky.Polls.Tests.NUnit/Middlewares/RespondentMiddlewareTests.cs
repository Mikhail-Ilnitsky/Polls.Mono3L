using System;
using System.Threading.Tasks;

using Ilnitsky.Polls.BusinessLogic;
using Ilnitsky.Polls.DataAccess.Entities.Answers;
using Ilnitsky.Polls.Middlewares;
using Ilnitsky.Polls.Tests.Shared;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

using Moq;

namespace Ilnitsky.Polls.Tests.NUnit.Middlewares;

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
        Assert.Multiple(() =>
        {
            // Проверяем, что в БД ничего не добавилось
            Assert.That(dbContext.Respondents, Is.Empty);
            // Проверяем что следующий Middleware был вызван
            Assert.That(wasNextCalled, Is.True);
        });
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
        Assert.Multiple(() =>
        {
            Assert.That(httpContext.Session.GetString("RespondentId"), Is.EqualTo(respondentId.ToString()));

            // Проверяем, что кука обновилась в ответе
            Assert.That(httpContext.Response.Headers.SetCookie.ToString(), Does.Contain("RespondentId"));
            // Убеждаемся, что запись как была одна, так и осталась
            Assert.That(dbContext.Respondents, Has.One.Items);

            Assert.That(wasNextCalled, Is.True);
        });
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
        var createdIdString = httpContext.Session.GetString("RespondentId");
        Assert.Multiple(() =>
        {
            Assert.That(createdIdString, Is.Not.Null);
            Assert.That(Guid.TryParse(createdIdString, out _), Is.True);
        });

        // Проверяем наличие в базе
        var respondentInDb = await dbContext.Respondents
            .FirstOrDefaultAsync(r => r.Id == Guid.Parse(createdIdString));
        Assert.Multiple(() =>
        {
            Assert.That(respondentInDb, Is.Not.Null);
            Assert.That(wasNextCalled, Is.True);
        });
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
        // Проверяем, что Middleware не упал, а создал новый валидный Id
        var sessionIdString = httpContext.Session.GetString("RespondentId");
        Assert.That(Guid.TryParse(sessionIdString, out var newGuid), Is.True);

        var respondentInDb = await dbContext.Respondents.AnyAsync(r => r.Id == newGuid);

        Assert.Multiple(() =>
        {
            // Проверяем, что в базе появилась запись с этим новым Id
            Assert.That(respondentInDb, Is.True);
            // Проверяем, что в ответе пришла кука с новым валидным Id
            Assert.That(httpContext.Response.Headers.SetCookie.ToString(), Does.Contain($"RespondentId={sessionIdString}"));

            Assert.That(wasNextCalled, Is.True);
        });
    }
}
