using System;
using System.Threading.Tasks;

using Ilnitsky.Polls.BusinessLogic;
using Ilnitsky.Polls.Contracts.Dtos.Polls;
using Ilnitsky.Polls.Services.DualCache;
using Ilnitsky.Polls.Services.OptionsProviders;
using Ilnitsky.Polls.Services.RedisCache;
using Ilnitsky.Polls.Services.Settings;
using Ilnitsky.Polls.Tests.Shared;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using Moq;

namespace Ilnitsky.Polls.Tests.NUnit.Services;

public class DualCacheServiceTests
{
    private MemoryCache _memoryCache;
    private Mock<IRedisCacheService> _redisCacheMock;
    private Mock<ILogger<RedisCacheService>> _loggerMock;
    private MemoryCacheOptionsProvider _memoryOptions;
    private RedisCacheOptionsProvider _redisOptions;

    [SetUp]
    public void Setup()
    {
        // Используем реальный MemoryCache для точности проверки
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _redisCacheMock = new Mock<IRedisCacheService>();
        _loggerMock = new Mock<ILogger<RedisCacheService>>();

        var memoryCacheSettings = new MemoryCacheSettings
        {
            DefaultExpirationSeconds = 300,
            PollExpirationSeconds = 600
        };
        var redisCacheSettings = new RedisCacheSettings
        {
            DefaultExpirationMinutes = 30,
            PollExpirationMinutes = 60
        };

        var memoryCacheOptions = Microsoft.Extensions.Options.Options.Create(memoryCacheSettings);
        var redisCacheOptions = Microsoft.Extensions.Options.Options.Create(redisCacheSettings);

        _memoryOptions = new MemoryCacheOptionsProvider(memoryCacheOptions);
        _redisOptions = new RedisCacheOptionsProvider(redisCacheOptions);
    }

    private DualCacheService CreateService() =>
        new(_memoryCache, _redisCacheMock.Object, _memoryOptions, _redisOptions, _loggerMock.Object);

    [Test]
    public async Task GetAsync_ReturnsFromMemory_WhenKeyExistsInMemory()
    {
        // Arrange
        var service = CreateService();
        var (pollEntity, pollId, pollKey) = TestDbHelper.CreatePoll();
        var pollDto = pollEntity.ToDto();

        _memoryCache.Set(pollKey, pollDto);

        // Act
        var result = await service.GetAsync<PollDto>(pollKey);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsRedisAvailable, Is.True);
            Assert.That(result.HasValue, Is.True);
            Assert.That(result.Value, Is.Not.Null);
        });
        Assert.Multiple(() =>
        {
            Assert.That(result.Value.PollId, Is.EqualTo(pollDto.PollId));
            Assert.That(result.Value.Name, Is.EqualTo(pollDto.Name));
        });

        // Проверяем, что Redis вообще не вызывался
        _redisCacheMock.Verify(
            x => x.GetAsync<PollDto>(It.IsAny<string>()),
            Times.Never);
    }

    [Test]
    public async Task GetAsync_ReturnsFromRedis_WhenKeyMissingInMemory()
    {
        // Arrange
        var service = CreateService();
        var (pollEntity, pollId, pollKey) = TestDbHelper.CreatePoll();
        var pollDto = pollEntity.ToDto();

        _redisCacheMock
            .Setup(x => x.GetAsync<PollDto>(pollKey))
            .ReturnsAsync(new RedisCacheResult<PollDto>(HasValue: true, Value: pollDto, IsRedisAvailable: true));

        // Act
        var result = await service.GetAsync<PollDto>(pollKey);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsRedisAvailable, Is.True);
            Assert.That(result.HasValue, Is.True);
            Assert.That(result.Value, Is.Not.Null);
        });
        Assert.Multiple(() =>
        {
            Assert.That(result.Value.PollId, Is.EqualTo(pollDto.PollId));
            Assert.That(result.Value.Name, Is.EqualTo(pollDto.Name));
        });
    }

    [Test]
    public async Task GetAsync_ReturnsHasValueFalse_WhenKeyMissingInBothCaches()
    {
        // Arrange
        var service = CreateService();
        var (_, _, pollKey) = TestDbHelper.CreatePoll();

        _redisCacheMock
            .Setup(x => x.GetAsync<PollDto>(pollKey))
            .ReturnsAsync(new RedisCacheResult<PollDto>(HasValue: false, Value: null, IsRedisAvailable: true));

        // Act
        var result = await service.GetAsync<PollDto>(pollKey);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsRedisAvailable, Is.True);
            Assert.That(result.HasValue, Is.False);
            Assert.That(result.Value, Is.Null);
        });

        // Проверяем, что Redis вызывался один раз
        _redisCacheMock.Verify(
            x => x.GetAsync<PollDto>(pollKey),
            Times.Once);
    }

    [TestCase(1)]
    [TestCase(10)]
    [TestCase(100)]
    [TestCase(1000)]
    public async Task SetAsync_SavesToCache_WhenMemoryExpirationIsNormalOrExceedsMax(int memoryExpirationMinutes)
    {
        // Arrange
        var service = CreateService();
        var (pollEntity, _, pollKey) = TestDbHelper.CreatePoll();
        var pollDto = pollEntity.ToDto();
        var memoryExpiration = TimeSpan.FromMinutes(memoryExpirationMinutes);

        // Act
        await service.SetAsync(pollKey, pollDto, true, memoryExpiration: memoryExpiration);

        // Assert
        var isValue = _memoryCache.TryGetValue<PollDto>(pollKey, out var value);

        Assert.Multiple(() =>
        {
            Assert.That(isValue, Is.True);
            Assert.That(value?.PollId, Is.EqualTo(pollDto.PollId));
        });
    }

    [Test]
    public async Task RemoveAsync_ClearsBothCaches()
    {
        // Arrange
        var service = CreateService();
        var (pollEntity, pollId, pollKey) = TestDbHelper.CreatePoll();
        var pollDto = pollEntity.ToDto();
        _memoryCache.Set(pollKey, pollDto);

        // Act
        await service.RemoveAsync(pollKey);

        // Assert
        Assert.That(_memoryCache.TryGetValue(pollKey, out _), Is.False);

        _redisCacheMock.Verify(
            x => x.RemoveAsync(pollKey),
            Times.Once);
    }

    [TearDown]
    public void TearDown()
    {
        _memoryCache.Dispose();
    }
}
