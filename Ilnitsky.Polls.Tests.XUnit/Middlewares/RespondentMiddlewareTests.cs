using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Ilnitsky.Polls.BusinessLogic;
using Ilnitsky.Polls.DataAccess;
using Ilnitsky.Polls.DataAccess.Entities.Answers;
using Ilnitsky.Polls.Middlewares;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Moq;

namespace Ilnitsky.Polls.Tests.XUnit.Middlewares;

public class RespondentMiddlewareTests
{
    private static DefaultHttpContext CreateHttpContext(ApplicationDbContext dbContext)
    {
        var context = new DefaultHttpContext();

        // Настраиваем ServiceProvider для получения DbContext через RequestServices
        var serviceProvider = new ServiceCollection()
            .AddSingleton(dbContext)
            .BuildServiceProvider();
        context.RequestServices = serviceProvider;

        // Мокаем сессию
        var sessionMock = new Mock<ISession>();
        var sessionData = new Dictionary<string, byte[]>();

        sessionMock
            .Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
            .Callback<string, byte[]>((key, val) => sessionData[key] = val);
        sessionMock
            .Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]?>.IsAny))
            .Returns((string key, out byte[]? val) => sessionData.TryGetValue(key, out val));
        sessionMock
            .Setup(s => s.Keys)
            .Returns(sessionData.Keys);

        context.Session = sessionMock.Object;
        return context;
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task InvokeAsync_CallsNextAndDoesNotModifyDb_WhenIdIsInSession()
    {
        // Arrange
        using var dbContext = CreateDbContext();

        var httpContext = CreateHttpContext(dbContext);
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
        using var dbContext = CreateDbContext();
        var respondentId = GuidHelper.CreateGuidV7();
        dbContext.Respondents.Add(new Respondent { Id = respondentId });
        await dbContext.SaveChangesAsync();

        var httpContext = CreateHttpContext(dbContext);
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
        using var dbContext = CreateDbContext();

        var httpContext = CreateHttpContext(dbContext);

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
        using var dbContext = CreateDbContext();
        var httpContext = CreateHttpContext(dbContext);

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
