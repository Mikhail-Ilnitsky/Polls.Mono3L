using System;
using System.Threading.Tasks;

using Ilnitsky.Polls.BusinessLogic;
using Ilnitsky.Polls.DataAccess.Entities.Answers;
using Ilnitsky.Polls.Middlewares;
using Ilnitsky.Polls.Tests.Shared;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

using Moq;

namespace Ilnitsky.Polls.Tests.XUnit.Middlewares;

public class RespondentMiddlewareTests
{
    [Fact]
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
        // Проверяем, что в БД ничего не добавилось
        Assert.Empty(dbContext.Respondents);
        // Проверяем что следующий Middleware был вызван
        Assert.True(wasNextCalled);
    }

    [Fact]
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
        Assert.Equal(respondentId.ToString(), httpContext.Session.GetString("RespondentId"));
        // Проверяем, что кука обновилась в ответе
        Assert.Contains("RespondentId", httpContext.Response.Headers.SetCookie.ToString());
        // Убеждаемся, что запись как была одна, так и осталась
        Assert.Single(dbContext.Respondents);

        Assert.True(wasNextCalled);
    }

    [Fact]
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
        Assert.NotNull(createdIdString);
        Assert.True(Guid.TryParse(createdIdString, out _));

        // Проверяем наличие в базе
        var respondentInDb = await dbContext.Respondents
            .FirstOrDefaultAsync(r => r.Id == Guid.Parse(createdIdString));
        Assert.NotNull(respondentInDb);

        Assert.True(wasNextCalled);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("not-a-guid-value")]
    [InlineData("1234567890")]
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
        var sessionIdString = httpContext.Session.GetString("RespondentId");

        // Проверяем, что Middleware не упал, а создал новый валидный Id
        Assert.NotNull(sessionIdString);
        Assert.True(Guid.TryParse(sessionIdString, out var newGuid));

        // Проверяем, что в базе появилась запись с этим новым Id
        var respondentInDb = await dbContext.Respondents.AnyAsync(r => r.Id == newGuid);
        Assert.True(respondentInDb);

        // Проверяем, что в ответе пришла кука с новым валидным Id
        Assert.Contains($"RespondentId={sessionIdString}", httpContext.Response.Headers.SetCookie.ToString());

        Assert.True(wasNextCalled);
    }
}
