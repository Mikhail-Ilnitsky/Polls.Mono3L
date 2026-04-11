using System;
using System.Linq;
using System.Threading.Tasks;

using Ilnitsky.Polls.BusinessLogic.Handlers.Polls;
using Ilnitsky.Polls.DataAccess;
using Ilnitsky.Polls.DbInitialization;

using Microsoft.EntityFrameworkCore;

namespace Ilnitsky.Polls.Tests.XUnit.Unit.Handlers;

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

    [Fact]
    public async Task HandleAsync_ReturnsSinglePoll_WhenSingleActivePollExists()
    {
        // Arrange
        var (poll, _, _) = TestDbHelper.CreatePoll();
        _dbContext.Polls.Add(poll);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _handler.HandleAsync(offset: 0, limit: 10);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(poll.Name, result[0].Name);
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
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        // Проверяем сортировку (OrderByDescending по DateTime)
        Assert.Equal(polls[1].Name, result[0].Name);
        Assert.Equal(polls[3].Name, result[1].Name);
    }

    [Fact]
    public async Task HandleAsync_ReturnsEmptyList_WhenOffsetIsTooHigh()
    {
        // Arrange
        var (poll, _, _) = TestDbHelper.CreatePoll();
        _dbContext.Polls.Add(poll);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _handler.HandleAsync(offset: 10, limit: 5);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task HandleAsync_ReturnsEmptyList_WhenDbIsEmpty()
    {
        // Arrange

        // Act
        var result = await _handler.HandleAsync(offset: 0, limit: 5);

        // Assert
        Assert.Empty(result);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }
}
