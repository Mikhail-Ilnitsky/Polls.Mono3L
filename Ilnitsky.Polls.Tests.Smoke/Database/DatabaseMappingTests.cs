using System.Threading.Tasks;

using FluentAssertions;

using Ilnitsky.Polls.DataAccess;
using Ilnitsky.Polls.Tests.Shared;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Ilnitsky.Polls.Tests.Smoke.Database;

public class DatabaseMappingTests
{
    private ApplicationDbContext _dbContext;
    private SqliteConnection _connection;

    [SetUp]
    public void Setup()
    {
        // Инициализируем объект соединения, указывая режим "в оперативной памяти" (:memory:)
        _connection = new SqliteConnection("Filename=:memory:");

        // Открываем соединение. В этот момент SQLite выделяет память под базу. 
        // База существует только пока это соединение открыто
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new ApplicationDbContext(options);

        // Заставляем EF Core прочитать OnModelCreating и построить таблицы в SQLite.
        // Если в маппинге есть ошибки, тест упадет уже на этой строке
        _dbContext.Database.EnsureCreated();
    }

    [TearDown]
    public async Task TearDown()
    {
        await _dbContext.DisposeAsync();
        _connection.Dispose();
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    [TestCase(5)]
    [TestCase(6)]
    [TestCase(7)]
    public async Task PollShouldBeCorrectlyPersistedToDatabase(int pollIndex)
    {
        // Arrange
        var createdPoll = TestDbHelper.CreatePollsList(pollIndex + 1)[pollIndex];

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
        );
    }
}
