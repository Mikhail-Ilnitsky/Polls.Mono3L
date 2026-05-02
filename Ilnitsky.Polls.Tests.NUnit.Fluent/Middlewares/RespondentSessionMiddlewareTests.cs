using System;
using System.Text;
using System.Threading.Tasks;

using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Extensions;

using Ilnitsky.Polls.BusinessLogic;
using Ilnitsky.Polls.Middlewares;
using Ilnitsky.Polls.Tests.Shared;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Ilnitsky.Polls.Tests.NUnit.Fluent.Middlewares;

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
        using (new AssertionScope())
        {
            var sessionIdString = httpContext.Session.GetString("RespondentSessionId");

            sessionIdString
                .Should().NotBeNull()
                .And.NotBeEmpty();
            Guid.TryParse(sessionIdString, out var sessionId)
                .Should().BeTrue();

            var sessionInDb = await dbContext.RespondentSessions
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            sessionInDb
                .Should().BeEquivalentTo(
                new
                {
                    RespondentId = respondentId,
                    RemoteIpAddress = "127.0.0.1",
                    UserAgent = "Mozilla/5.0",
                    AcceptLanguage = "en-US,en;q=0.9",
                    Platform = "Windows",
                    Brand = "Chromium",
                    IsMobile = true
                });
            sessionInDb.DateTime
                .Should().BeCloseTo(DateTime.UtcNow, 5.Seconds());
            wasNextCalled
                .Should().BeTrue();
        }
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
        using (new AssertionScope())
        {
            dbContext.RespondentSessions
                .Should().BeEmpty();
            wasNextCalled
                .Should().BeTrue();
        }
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

        var middleware = new RespondentSessionMiddleware(innerContext => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        using (new AssertionScope())
        {
            var sessionInDb = await dbContext.RespondentSessions.FirstAsync();
            sessionInDb.RemoteIpAddress
                .Should().BeEmpty();
        }
    }
}
