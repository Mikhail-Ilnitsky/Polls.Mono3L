using System;
using System.Linq;

using FluentAssertions;

using Ilnitsky.Polls.BusinessLogic.Handlers.Answers;
using Ilnitsky.Polls.BusinessLogic.Handlers.Polls;
using Ilnitsky.Polls.DataAccess;
using Ilnitsky.Polls.Services.DualCache;
using Ilnitsky.Polls.Services.OptionsProviders;
using Ilnitsky.Polls.Services.RedisCache;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ilnitsky.Polls.Tests.Smoke.DependencyInjection;

public class DependencyRegistrationTests
{
    private static WebApplicationFactory<Program> CreateFactory()
    {
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
                        options.UseSqlite("Filename=:memory:"));
                });
            });
    }

    [Test]
    public void AppDatabaseContext_IsResolvable()
    {
        // Arrange
        using var factory = CreateFactory();

        // Act & Assert
        using var scope = factory.Services.CreateScope();
        scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>()
            .Should().NotBeNull();
    }

    [TestCase(typeof(IRedisCacheService))]
    [TestCase(typeof(IDualCacheService))]
    [TestCase(typeof(RedisCacheOptionsProvider))]
    [TestCase(typeof(MemoryCacheOptionsProvider))]
    [TestCase(typeof(IGetPollByIdHandler))]
    [TestCase(typeof(IGetPollLinksHandler))]
    [TestCase(typeof(ICreateRespondentAnswerHandler))]
    public void AppRequiredService_IsResolvable(Type serviceType)
    {
        // Arrange
        using var factory = CreateFactory();

        // Act & Assert
        using var scope = factory.Services.CreateScope();
        scope.ServiceProvider
            .GetRequiredService(serviceType)
            .Should().NotBeNull();
    }
}
