using System;
using System.Linq;
using System.Threading.Tasks;

using Ilnitsky.Polls.BusinessLogic.Handlers.Polls;
using Ilnitsky.Polls.DataAccess;
using Ilnitsky.Polls.DbInitialization;
using Ilnitsky.Polls.Tests.Shared;

using Microsoft.EntityFrameworkCore;

namespace Ilnitsky.Polls.Tests.NUnit.Handlers;

public class GetPollLinksHandlerTests
{
    private ApplicationDbContext _dbContext;
    private GetPollLinksHandler _handler;

    [SetUp]
    public void Setup()
    {
        // Создаем уникальное имя БД для каждого запуска теста
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _handler = new GetPollLinksHandler(_dbContext);
    }

    [TestCase(1)]
    [TestCase(10)]
    [TestCase(100)]
    public async Task HandleAsync_ReturnsSinglePoll_WhenSingleActivePollExists(int limit)
    {
        // Arrange
        var (poll, _, _) = TestDbHelper.CreatePoll();
        _dbContext.Polls.Add(poll);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _handler.HandleAsync(offset: 0, limit: limit);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Has.One.Items);
        Assert.That(result[0].Name, Is.EqualTo(poll.Name));
    }

    [Test]
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
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Has.Count.EqualTo(2));

        // Проверяем сортировку(OrderByDescending по DateTime)
        Assert.Multiple(() =>
        {
            Assert.That(result[0].Name, Is.EqualTo(polls[1].Name));
            Assert.That(result[1].Name, Is.EqualTo(polls[3].Name));
        });
    }

    [TestCase(1)]
    [TestCase(10)]
    [TestCase(100)]
    public async Task HandleAsync_ReturnsEmptyList_WhenOffsetIsTooHigh(int offset)
    {
        // Arrange
        var (poll, _, _) = TestDbHelper.CreatePoll();
        _dbContext.Polls.Add(poll);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _handler.HandleAsync(offset, limit: 5);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [TestCase(0, 5)]
    [TestCase(0, 10)]
    [TestCase(5, 5)]
    [TestCase(10, 10)]
    public async Task HandleAsync_ReturnsEmptyList_WhenDbIsEmpty(int offset, int limit)
    {
        // Arrange

        // Act
        var result = await _handler.HandleAsync(offset, limit);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
    }
}
