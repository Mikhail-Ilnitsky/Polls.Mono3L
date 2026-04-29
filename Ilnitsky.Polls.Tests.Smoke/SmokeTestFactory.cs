using System.Linq;

using Ilnitsky.Polls.DataAccess;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ilnitsky.Polls.Tests.Smoke;

public static class SmokeTestFactory
{
    private static readonly object _lock = new();
    private static WebApplicationFactory<Program>? _factory;
    private static SqliteConnection? _keepAliveConnection;

    private static WebApplicationFactory<Program> CreateFactory()
    {
        // Создаем и открываем соединение ДО создания фабрики
        _keepAliveConnection = new SqliteConnection("DataSource=:memory:");
        _keepAliveConnection.Open();

        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                // Добавляем в конфигурацию настройку, заставляющую пропустить шаг миграции
                builder.UseSetting("SkipMigrations", "true");

                // Подменяем MySQL на SQLite (чтобы не падал при создании DbContext)
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseSqlite(_keepAliveConnection));
                });
            });
    }

    public static WebApplicationFactory<Program> GetInstance()
    {
        if (_factory != null)
        {
            return _factory;
        }

        lock (_lock)
        {
            _factory ??= CreateFactory();
        }

        return _factory;
    }

    public static void DisposeInstance()
    {
        _factory?.Dispose();
        _keepAliveConnection?.Close();
        _keepAliveConnection?.Dispose();
    }
}
