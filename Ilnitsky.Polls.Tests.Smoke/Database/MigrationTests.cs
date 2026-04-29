using System.Linq;

using FluentAssertions;

using Ilnitsky.Polls.DataAccess;
using Ilnitsky.Polls.DataAccess.Migrations;

using Microsoft.EntityFrameworkCore;

namespace Ilnitsky.Polls.Tests.Smoke.Database;

public class MigrationTests
{
    private class TestSnapshot : ApplicationDbContextModelSnapshot
    {
        public void ExposeBuildModel(ModelBuilder modelBuilder)
        {
            base.BuildModel(modelBuilder);
        }
    }

    [Test]
    public void DatabaseModelMatchesSnapshotByStructure()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite("Filename=:memory:")
            .Options;

        using var context = new ApplicationDbContext(options);

        // Получаем упрощенную структуру текущей модели в коде
        var currentStructure = context.Model.GetEntityTypes()
            .Select(e => new
            {
                e.Name,
                Props = e.GetProperties().Select(p => p.Name).OrderBy(n => n)
            })
            .OrderBy(e => e.Name);

        var snapshot = new TestSnapshot();
        var modelBuilder = new ModelBuilder();
        snapshot.ExposeBuildModel(modelBuilder);

        // Получаем структуру из Snapshot
        var snapshotStructure = modelBuilder.Model
            .GetEntityTypes()
            .Select(e => new
            {
                e.Name,
                Props = e.GetProperties().Select(p => p.Name).OrderBy(n => n)
            })
            .OrderBy(e => e.Name);

        // Act & Assert
        // Сравниваем списки таблиц и имена колонок в них
        currentStructure
            .Should().BeEquivalentTo(
                snapshotStructure,
                "Cостав таблиц или полей в коде не совпадает с последней миграцией!");
    }
}
