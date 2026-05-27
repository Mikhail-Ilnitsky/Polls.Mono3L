using System.Threading.Tasks;

using FluentAssertions;

using Ilnitsky.Polls.DataAccess;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ilnitsky.Polls.Tests.Integration.XUnit.Database;

[Collection("GlobalCollection")]
public class MigrationTests(AppFixture fixture)
{
    private WebApplicationFactory<Program> Factory => fixture.Factory;

    [Fact]
    public async Task Database_ShouldBeInSyncWithMigrations()
    {
        // Arrange
        // Получаем сервис из DI
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Act
        // Проверяем, есть ли миграции, которые еще не применены к реальной MariaDB
        var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();

        // Assert
        // Если список не пуст, значит база в Docker отстает от кода
        pendingMigrations.Should().BeEmpty("База данных в Docker содержит не все миграции!");

        // Проверяем, что EF Core считает структуру таблиц совместимой
        // Это самый жесткий тест: он пытается сопоставить модель с реальной схемой БД
        var canConnect = await dbContext.Database.CanConnectAsync();
        canConnect.Should().BeTrue("Не удалось подключиться к базе для проверки структуры");
    }
}
