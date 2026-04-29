using System;

using FluentAssertions;

using Ilnitsky.Polls.BusinessLogic.Handlers.Answers;
using Ilnitsky.Polls.BusinessLogic.Handlers.Polls;
using Ilnitsky.Polls.DataAccess;
using Ilnitsky.Polls.Services.DualCache;
using Ilnitsky.Polls.Services.OptionsProviders;
using Ilnitsky.Polls.Services.RedisCache;

using Microsoft.Extensions.DependencyInjection;

namespace Ilnitsky.Polls.Tests.Smoke.DependencyInjection;

public class DependencyRegistrationTests
{
    [Test]
    public void AppDatabaseContext_IsResolvable()
    {
        // Arrange
        var factory = SmokeTestFactory.GetInstance();

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
        var factory = SmokeTestFactory.GetInstance();

        // Act & Assert
        using var scope = factory.Services.CreateScope();
        scope.ServiceProvider
            .GetRequiredService(serviceType)
            .Should().NotBeNull();
    }
}
