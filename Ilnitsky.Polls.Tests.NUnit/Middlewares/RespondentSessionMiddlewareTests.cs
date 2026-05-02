using System;
using System.Text;
using System.Threading.Tasks;

using Ilnitsky.Polls.BusinessLogic;
using Ilnitsky.Polls.Middlewares;
using Ilnitsky.Polls.Tests.Shared;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Ilnitsky.Polls.Tests.NUnit.Middlewares;

public class RespondentSessionMiddlewareTests
{
    [Test]
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
        Assert.That(sessionIdString, Is.Not.Null);

        var sessionInDb = await dbContext.RespondentSessions
            .FirstOrDefaultAsync(s => s.Id == Guid.Parse(sessionIdString));

        Assert.That(sessionInDb, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(sessionInDb.RespondentId, Is.EqualTo(respondentId));
            Assert.That(sessionInDb.RemoteIpAddress, Is.EqualTo("127.0.0.1"));
            Assert.That(sessionInDb.UserAgent, Is.EqualTo("Mozilla/5.0"));
            Assert.That(sessionInDb.AcceptLanguage, Is.EqualTo("en-US,en;q=0.9"));
            Assert.That(sessionInDb.Platform, Is.EqualTo("Windows"));
            Assert.That(sessionInDb.Brand, Is.EqualTo("Chromium"));
            Assert.That(sessionInDb.IsMobile, Is.True);
            Assert.That((DateTime.UtcNow - sessionInDb.DateTime).TotalSeconds, Is.LessThan(5));

            Assert.That(wasNextCalled, Is.True);
        });
    }

    [Test]
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
        Assert.Multiple(() =>
        {
            // В базе не должно появиться записей
            Assert.That(dbContext.RespondentSessions, Is.Empty);
            Assert.That(wasNextCalled, Is.True);
        });
    }

    [Test]
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
        Assert.That(sessionInDb.RemoteIpAddress, Is.Empty);
    }
}
