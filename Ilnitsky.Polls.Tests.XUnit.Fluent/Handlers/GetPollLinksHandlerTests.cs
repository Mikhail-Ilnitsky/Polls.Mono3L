using System;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using Ilnitsky.Polls.BusinessLogic.Handlers.Polls;
using Ilnitsky.Polls.DataAccess;
using Ilnitsky.Polls.DbInitialization;
using Ilnitsky.Polls.Tests.Shared;

using Microsoft.EntityFrameworkCore;

namespace Ilnitsky.Polls.Tests.XUnit.Fluent.Handlers;

public class GetPollLinksHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetPollLinksHandler _handler;

    public GetPollLinksHandlerTests()
    {
        // Создаем уникальное имя БД для каждого запуска теста
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _handler = new GetPollLinksHandler(_dbContext);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task HandleAsync_ReturnsSinglePoll_WhenSingleActivePollExists(int limit)
    {
        // Arrange
        var (poll, _, _) = TestDbHelper.CreatePoll();
        _dbContext.Polls.Add(poll);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _handler.HandleAsync(offset: 0, limit: limit);

        // Assert
        result.Should().BeEquivalentTo(
            [
                new
                {
                    poll.Name
                }
            ]);
    }

    [Fact]
    public async Task HandleAsync_ReturnsActivePolls_WithCorrectPagination()
    {
        // Arrange
        var now = DateTime.Now;
        var polls = DbInitializer
            .CreatePolls()
            .Take(5)
            .ToList();

        polls[0].IsActive = false;
        polls[0].DateTime = now;

        polls[1].DateTime = now.AddMinutes(-5);

        polls[2].IsActive = false;
        polls[2].DateTime = now.AddMinutes(-10);

        polls[3].DateTime = now.AddMinutes(-15);

        polls[4].DateTime = now.AddMinutes(-20);

        _dbContext.Polls.AddRange(polls);
        await _dbContext.SaveChangesAsync();

        int offset = 0;
        int limit = 2;

        // Act
        var result = await _handler.HandleAsync(offset, limit);

        // Assert
        result.Should().BeEquivalentTo(
            [
                new
                {
                    polls[1].Name
                },
                new
                {
                    polls[3].Name
                }
            ],
            options => options.WithStrictOrdering());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task HandleAsync_ReturnsEmptyList_WhenOffsetIsTooHigh(int offset)
    {
        // Arrange
        var (poll, _, _) = TestDbHelper.CreatePoll();
        _dbContext.Polls.Add(poll);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _handler.HandleAsync(offset, limit: 5);

        // Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0, 5)]
    [InlineData(0, 10)]
    [InlineData(5, 5)]
    [InlineData(10, 10)]
    public async Task HandleAsync_ReturnsEmptyList_WhenDbIsEmpty(int offset, int limit)
    {
        // Arrange

        // Act
        var result = await _handler.HandleAsync(offset, limit);

        // Assert
        result.Should().BeEmpty();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }
}
