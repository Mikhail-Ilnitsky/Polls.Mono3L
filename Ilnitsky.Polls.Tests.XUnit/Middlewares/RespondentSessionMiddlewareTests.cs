using System;
using System.Text;
using System.Threading.Tasks;

using Ilnitsky.Polls.BusinessLogic;
using Ilnitsky.Polls.Middlewares;
using Ilnitsky.Polls.Tests.Shared;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Ilnitsky.Polls.Tests.XUnit.Middlewares;

public class RespondentSessionMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_CreatesNewSession_WhenSessionIdIsMissing()
    {
        // Arrange
        using var dbContext = ContextHelper.CreateDbContext();
        var httpContext = ContextHelper.CreateHttpContext(dbContext);

        // Предварительно записываем RespondentId, так как Middleware его ожидает
        var respondentId = GuidHelper.CreateGuidV7();
        httpContext.Session.SetString("RespondentId", respondentId.ToString());

        // Имитируем данные браузера
        httpContext.Request.Headers.UserAgent = "Mozilla/5.0";
        httpContext.Request.Headers.AcceptLanguage = "en-US,en;q=0.9";
        httpContext.Request.Headers["sec-ch-ua-mobile"] = "?1";
        httpContext.Request.Headers["sec-ch-ua-platform"] = "Windows";
        httpContext.Request.Headers["sec-ch-ua"] = "Chromium";
        httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");

        bool wasNextCalled = false;
        var middleware = new RespondentSessionMiddleware(innerContext =>
        {
            wasNextCalled = true;
            return Task.CompletedTask;
        });

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        var sessionIdString = httpContext.Session.GetString("RespondentSessionId");
        Assert.NotNull(sessionIdString);

        var sessionInDb = await dbContext.RespondentSessions
            .FirstOrDefaultAsync(s => s.Id == Guid.Parse(sessionIdString));

        Assert.NotNull(sessionInDb);
        Assert.Equal(respondentId, sessionInDb.RespondentId);
        Assert.Equal("127.0.0.1", sessionInDb.RemoteIpAddress);
        Assert.Equal("Mozilla/5.0", sessionInDb.UserAgent);
        Assert.Equal("en-US,en;q=0.9", sessionInDb.AcceptLanguage);
        Assert.Equal("Windows", sessionInDb.Platform);
        Assert.Equal("Chromium", sessionInDb.Brand);
        Assert.True(sessionInDb.IsMobile);
        Assert.True((DateTime.UtcNow - sessionInDb.DateTime).TotalSeconds < 5);

        Assert.True(wasNextCalled);
    }

    [Fact]
    public async Task InvokeAsync_DoesNotCreateNewSession_WhenSessionIdAlreadyExists()
    {
        // Arrange
        using var dbContext = ContextHelper.CreateDbContext();
        var httpContext = ContextHelper.CreateHttpContext(dbContext);

        var existingSessionId = GuidHelper.CreateGuidV7();
        httpContext.Session.SetString("RespondentSessionId", existingSessionId.ToString());

        bool wasNextCalled = false;
        var middleware = new RespondentSessionMiddleware(innerContext =>
        {
            wasNextCalled = true;
            return Task.CompletedTask;
        });

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        // В базе не должно появиться записей
        Assert.Empty(dbContext.RespondentSessions);
        Assert.True(wasNextCalled);
    }

    [Fact]
    public async Task InvokeAsync_UsesEmptyString_WhenRemoteIpAddressIsNull()
    {
        // Arrange
        using var dbContext = ContextHelper.CreateDbContext();
        var httpContext = ContextHelper.CreateHttpContext(dbContext);
        httpContext.Session.SetString("RespondentId", GuidHelper.CreateGuidV7().ToString());

        // Явно зануляем IP (хотя он и так null в DefaultHttpContext по умолчанию)
        httpContext.Connection.RemoteIpAddress = null;

        var middleware = new RespondentSessionMiddleware(next => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        var sessionInDb = await dbContext.RespondentSessions.FirstAsync();
        Assert.Equal(string.Empty, sessionInDb.RemoteIpAddress);
    }
}
