using System;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using Ilnitsky.Polls.BusinessLogic;
using Ilnitsky.Polls.DataAccess;
using Ilnitsky.Polls.Tests.Shared;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ilnitsky.Polls.Tests.Integration.XUnit.Database;

[Collection("GlobalCollection")]
public class DatabaseMappingTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IServiceScope _scope;

    public DatabaseMappingTests(AppFixture fixture)
    {
        // Создаем область(Scope), чтобы получать чистый DbContext для каждого теста
        _scope = fixture.Factory.Services.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _scope.Dispose();
        GC.SuppressFinalize(this);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
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
