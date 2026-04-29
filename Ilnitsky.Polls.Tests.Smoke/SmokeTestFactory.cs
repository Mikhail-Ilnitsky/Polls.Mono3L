using System.Linq;

using Ilnitsky.Polls.DataAccess;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Ilnitsky.Polls.Tests.Smoke;

public static class SmokeTestFactory
{
    private static readonly object _lock = new();
    private static WebApplicationFactory<Program>? _factory;
    private static SqliteConnection? _keepAliveConnection;

    private static WebApplicationFactory<Program> CreateFactory()
    {
        // Создаем и открываем соединение ДО создания фабрики
        var keepAliveConnection = new SqliteConnection("DataSource=:memory:");
        keepAliveConnection.Open();

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
                        options.UseSqlite(keepAliveConnection));

                    services.PostConfigure<HealthCheckServiceOptions>(options =>
                    {
                        // Очищаем все реальные проверки
                        options.Registrations.Clear();
                    });

                    services
                        .AddHealthChecks()
                        .AddCheck("Self", () => HealthCheckResult.Healthy());
                });
            });
    }

    public static WebApplicationFactory<Program> GetInstance() => CreateFactory();
    //{
    //    if (_factory != null)
    //    {
    //        return _factory;
    //    }

    //    lock (_lock)
    //    {
    //        _factory ??= CreateFactory();
    //    }

    //    return _factory;
    //}

    public static void DisposeInstance()
    {
        _factory?.Dispose();
        _keepAliveConnection?.Close();
        _keepAliveConnection?.Dispose();
    }
}
