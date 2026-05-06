using Microsoft.AspNetCore.Mvc.Testing;

using Testcontainers.MariaDb;
using Testcontainers.Redis;

namespace Ilnitsky.Polls.Tests.Integration.NUnit;

[SetUpFixture]
public class GlobalTestsSetup
{
    public static WebApplicationFactory<Program> Factory { private set; get; } = null!;
    public static HttpClient HttpClient { private set; get; } = null!;
    public static string DbConnectionString { private set; get; } = null!;

    private static readonly MariaDbContainer MariaDbContainer = new MariaDbBuilder()
        .WithImage("mariadb:11.8.2-noble")
        .WithDatabase("polls_test_db")
        .WithUsername("test_user")
        .WithPassword("test_password")
        .Build();

    private static readonly RedisContainer RedisContainer = new RedisBuilder()
        .WithImage("redis:8.0.2-alpine3.21")
        .Build();

    [OneTimeSetUp]
    public async Task RunBeforeAnyTestsAsync()
    {
        // Запускаем контейнеры (MariaDB + Redis)
        await Task.WhenAll(MariaDbContainer.StartAsync(), RedisContainer.StartAsync());

        // Получаем валидную строку подключения для MariaDB
        DbConnectionString = MariaDbContainer.GetConnectionString();
        var redisConnectionString = RedisContainer.GetConnectionString();

        // Подменяем конфигурацию в приложении
        //Factory = new WebApplicationFactory<Program>()
        //    .WithWebHostBuilder(hostBuilder =>
        //    {
        //        hostBuilder.ConfigureAppConfiguration((hostBuilderContext, configurationBuilder) =>
        //        {
        //            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        //            {
        //                ["ConnectionStrings:DefaultConnection"] = DbConnectionString,
        //                ["ConnectionStrings:Redis"] = RedisContainer.GetConnectionString()
        //            });
        //        });
        //    });

        // Устанавливаем переменные окружения напрямую в процесс тестов
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", DbConnectionString);
        Environment.SetEnvironmentVariable("ConnectionStrings__Redis", redisConnectionString);

        Factory = new WebApplicationFactory<Program>();
        HttpClient = Factory.CreateClient();
    }

    [OneTimeTearDown]
    public async Task RunAfterAllTestsAsync()
    {
        // Очищаем переменные
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", null);
        Environment.SetEnvironmentVariable("ConnectionStrings__Redis", null);

        HttpClient?.Dispose();

        if (Factory != null)
        {
            await Factory.DisposeAsync();
        }

        // Уничтожаем контейнеры
        await MariaDbContainer.DisposeAsync();
        await RedisContainer.DisposeAsync();
    }
}
