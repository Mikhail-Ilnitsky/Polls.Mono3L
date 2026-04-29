using System.Linq;
using System.Net;
using System.Threading.Tasks;

using FluentAssertions;

using Ilnitsky.Polls.DataAccess;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Ilnitsky.Polls.Tests.Smoke.Hosting;

public class AppHostingTests
{
    private static SqliteConnection CreateConnection()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        return connection;
    }

    private static WebApplicationFactory<Program> CreateFactory(SqliteConnection connection)
    {
        // Создаем и открываем соединение ДО создания фабрики
        //var keepAliveConnection = new SqliteConnection("DataSource=:memory:");
        //keepAliveConnection.Open();

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
                        options.UseSqlite(connection));

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

    [Test]
    public async Task AppHealthCheckEndpoint_ReturnsHealthy()
    {
        // Arrange
        using var connection = CreateConnection();
        using var factory = CreateFactory(connection);
        var httpClient = factory.CreateClient();

        // Act
        var response = await httpClient.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().BeOneOf("Healthy", "Degraded");
    }

    [Test]
    public async Task AppLivenessEndpoint_ReturnsHealthy()
    {
        // Arrange
        using var connection = CreateConnection();
        using var factory = CreateFactory(connection);
        var httpClient = factory.CreateClient();

        // Act
        var response = await httpClient.GetAsync("/health/live");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Healthy");
    }

    [Test]
    public async Task AppReadinessEndpoint_ReturnsOk()
    {
        // Arrange
        using var connection = CreateConnection();
        using var factory = CreateFactory(connection);
        var httpClient = factory.CreateClient();

        // Act
        var response = await httpClient.GetAsync("/health/ready");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Test]
    public async Task AppMetricsEndpoint_ReturnsData()
    {
        // Arrange
        using var connection = CreateConnection();
        using var factory = CreateFactory(connection);
        var httpClient = factory.CreateClient();

        // Act
        var response = await httpClient.GetAsync("/metrics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty(); // Проверяем, что метрики генерируются
    }
}
