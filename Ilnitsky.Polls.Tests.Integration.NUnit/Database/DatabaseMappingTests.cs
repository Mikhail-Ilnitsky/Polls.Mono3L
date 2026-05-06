using FluentAssertions;

using Ilnitsky.Polls.BusinessLogic;
using Ilnitsky.Polls.DataAccess;
using Ilnitsky.Polls.Tests.Shared;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ilnitsky.Polls.Tests.Integration.NUnit.Database;

public class DatabaseMappingTests
{
    private ApplicationDbContext _dbContext;
    private IServiceScope _scope;

    [SetUp]
    public void Setup()
    {
        // Создаем область(Scope), чтобы получать чистый DbContext для каждого теста
        _scope = GlobalTestsSetup.Factory.Services.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }

    [TearDown]
    public async Task TearDown()
    {
        await _dbContext.DisposeAsync();
        _scope.Dispose();
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    [TestCase(5)]
    [TestCase(6)]
    [TestCase(7)]
    public async Task Poll_ShouldBeCorrectlyPersisted_ToRealMariaDb(int pollIndex)
    {
        // Arrange
        var createdPoll = TestDbHelper.CreatePollsList(pollIndex + 1)[pollIndex];
        var createdQuestion = createdPoll.Questions.First();
        var createdAnswers = createdQuestion.Answers.ToList();

        createdPoll.Id = GuidHelper.CreateGuidV7();
        createdPoll.Name += "_тест!";

        createdQuestion.Id = GuidHelper.CreateGuidV7();
        createdQuestion.Text += "_тест!";

        createdAnswers.ForEach(a =>
        {
            a.Id = GuidHelper.CreateGuidV7();
            a.Text += "_тест!";
        });

        // Act
        _dbContext.Polls.Add(createdPoll);
        await _dbContext.SaveChangesAsync();

        // Очищаем кэш, чтобы загрузить из БД
        _dbContext.ChangeTracker.Clear();

        var dbPoll = await _dbContext.Polls
            .Include(p => p.Questions)
                .ThenInclude(q => q.Answers)
            .FirstOrDefaultAsync(p => p.Id == createdPoll.Id);

        // Assert
        dbPoll.Should().BeEquivalentTo(
            createdPoll,
            options => options
                .IgnoringCyclicReferences()
                .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromMilliseconds(1)))
                .WhenTypeIs<DateTime>()
        );
    }
}
