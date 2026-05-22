using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using Ilnitsky.Polls.Contracts.Dtos.Polls;
using Ilnitsky.Polls.DataAccess;
using Ilnitsky.Polls.Tests.Shared;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using StackExchange.Redis;

namespace Ilnitsky.Polls.Tests.Integration.NUnit.Api;

public class PollsApiTests
{
    private static HttpClient HttpClient => GlobalTestsSetup.HttpClient;

    // Некорректные/незаданные offset и limit
    [TestCase(null, null, 5)]
    [TestCase(null, -1, 5)]
    [TestCase(-1, null, 5)]
    [TestCase(-1, -1, 5)]
    [TestCase(-10, -10, 5)]
    // Некорректный/незаданный offset
    [TestCase(null, 1, 1)]
    [TestCase(null, 7, 7)]
    [TestCase(-1, 1, 1)]
    [TestCase(-1, 7, 7)]
    [TestCase(-10, 1, 1)]
    [TestCase(-10, 7, 7)]
    // Некорректный/незаданный limit
    [TestCase(0, null, 5)]
    [TestCase(0, 0, 5)]
    [TestCase(0, -1, 5)]
    [TestCase(0, -10, 5)]
    [TestCase(3, null, 5)]
    [TestCase(3, 0, 5)]
    [TestCase(3, -1, 5)]
    [TestCase(3, -10, 5)]
    // Разный корректный limit
    [TestCase(0, 1, 1)]
    [TestCase(0, 3, 3)]
    [TestCase(0, 7, 7)]
    [TestCase(0, 11, 11)]
    // Разный корректный offset
    [TestCase(0, 3, 3)]
    [TestCase(2, 3, 3)]
    [TestCase(4, 3, 3)]
    [TestCase(6, 3, 3)]
    // Корректный offset больше чем количество строк в БД
    [TestCase(50, 3, 0)]
    [TestCase(100, 3, 0)]
    public async Task GetPollLinks_ReturnsCorrectCount_FromRealDatabase(int? offset, int? limit, int resultPollsCount)
    {
        var queryParams = new Dictionary<string, string?>();
        if (offset.HasValue)
        {
            queryParams.Add("offset", offset.ToString());
        }
        if (limit.HasValue)
        {
            queryParams.Add("limit", limit.ToString());
        }
        var url = QueryHelpers.AddQueryString("api/v1/polls", queryParams);

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var pollsLinks = await response.Content.ReadFromJsonAsync<List<PollLinkDto>>();
        pollsLinks.Should().HaveCount(resultPollsCount);
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    [TestCase(5)]
    public async Task GetPollById_ReturnsOk_WhenPollExists(int pollIndex)
    {
        // Arrange
        using var scope = GlobalTestsSetup.Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var existingPoll = await dbContext.Polls
            .Skip(pollIndex)
            .FirstAsync();
        var existingId = existingPoll.Id;

        // Act
        var response = await HttpClient.GetAsync($"api/v1/polls/{existingId}");

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var poll = await response.Content.ReadFromJsonAsync<PollDto>();
        poll.Should().BeEquivalentTo(
            new
            {
                PollId = existingId,
                existingPoll.Name,
                existingPoll.DateTime,
                existingPoll.IsActive,
                existingPoll.Html
            });
    }

    [Test]
    public async Task GetPollById_ReturnsNotFound_WhenPollDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await HttpClient.GetAsync($"api/v1/polls/{nonExistentId}");

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem!.Status.Should().Be(404);
        problem.Title.Should().Be("Ошибка!");
        problem.Detail.Should().Contain("Опрос не найден");
    }

    [Test]
    public async Task GetPollById_ReturnsInternalServerError_OnHackGuid()
    {
        // Arrange
        var hackId = Guid.Parse("019c1aa8-9bf0-750d-9e6d-832de94b1c13");

        // Act
        var response = await HttpClient.GetAsync($"api/v1/polls/{hackId}");

        // Assert
        response.Should().NotBeNull();
        // Клиент должен получить код 500, а не падение процесса
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem!.Status.Should().Be(500);
        problem.Title.Should().Be("Ошибка!");
        problem.Detail.Should().Contain("Внутренняя ошибка сервера");
    }

    [Test]
    public async Task GetPollById_CachesResultInRedis_AfterFirstCall()
    {
        // Arrange
        using var scope = GlobalTestsSetup.Factory.Services.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var (poll, pollId, pollKey) = TestDbHelper.CreatePoll();
        dbContext.Polls.Add(poll);
        await dbContext.SaveChangesAsync();

        var redisConnectionMultiplexer = GlobalTestsSetup.Factory.Services.GetRequiredService<IConnectionMultiplexer>();
        var redisDb = redisConnectionMultiplexer.GetDatabase();

        // Act
        await HttpClient.GetAsync($"api/v1/polls/{pollId}");

        // Assert
        var isCachedData = await redisDb.KeyExistsAsync(pollKey);
        isCachedData.Should().BeTrue("Данные должны сохраниться в Redis для ускорения");
    }
}
