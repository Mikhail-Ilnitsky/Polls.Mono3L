using System;
using System.Net.Http;
using System.Threading.Tasks;

using DotNet.Testcontainers.Builders;

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
        .WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted("mariadb-admin ping"))
        .Build();

    private static readonly RedisContainer RedisContainer = new RedisBuilder()
        .WithImage("redis:8.0.2-alpine3.21")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(6379))
        .Build();

    [OneTimeSetUp]
    public async Task RunBeforeAnyTestsAsync()
    {
        // Запускаем контейнеры (MariaDB + Redis) и дожидаемся их готовности
        await Task.WhenAll(MariaDbContainer.StartAsync(), RedisContainer.StartAsync());

        // Получаем валидные строки подключения для MariaDB и Redis
        DbConnectionString = MariaDbContainer.GetConnectionString();
        var redisConnectionString = RedisContainer.GetConnectionString() + ",abortConnect=false,connectTimeout=5000,syncTimeout=5000";

        // Устанавливаем переменные окружения напрямую в процесс тестов
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", DbConnectionString);
        Environment.SetEnvironmentVariable("ConnectionStrings__Redis", redisConnectionString);

        // Устанавливаем среду Testing, чтобы не настраивался и не запускался Swagger
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");

        Factory = new WebApplicationFactory<Program>();
        HttpClient = Factory.CreateClient();
    }

    [OneTimeTearDown]
    public async Task RunAfterAllTestsAsync()
    {
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
