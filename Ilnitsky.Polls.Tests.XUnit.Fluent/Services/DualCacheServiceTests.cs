using System;
using System.Threading.Tasks;

using FluentAssertions;

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

namespace Ilnitsky.Polls.Tests.XUnit.Fluent.Services;

public class DualCacheServiceTests : IDisposable
{
    private MemoryCache _memoryCache;
    private Mock<IRedisCacheService> _redisCacheMock;
    private Mock<ILogger<RedisCacheService>> _loggerMock;
    private MemoryCacheOptionsProvider _memoryOptions;
    private RedisCacheOptionsProvider _redisOptions;

    public DualCacheServiceTests()
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

    [Fact]
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
        result.Should().BeEquivalentTo(new
        {
            IsRedisAvailable = true,
            HasValue = true,
            Value = pollDto
        });

        // Проверяем, что Redis вообще не вызывался
        _redisCacheMock.Verify(
            x => x.GetAsync<PollDto>(It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
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
        result.Should().BeEquivalentTo(new
        {
            IsRedisAvailable = true,
            HasValue = true,
            Value = pollDto
        });
    }

    [Fact]
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
        result.Should().BeEquivalentTo(new
        {
            IsRedisAvailable = true,
            HasValue = false,
            Value = (object?)null
        });

        // Проверяем, что Redis вызывался один раз
        _redisCacheMock.Verify(
            x => x.GetAsync<PollDto>(pollKey),
            Times.Once);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
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

        isValue.Should().BeTrue();
        value.Should().NotBeNull();
        value.PollId.Should().Be(pollDto.PollId);
    }

    [Fact]
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
        var isValue = _memoryCache.TryGetValue(pollKey, out _);
        isValue.Should().BeFalse();

        _redisCacheMock.Verify(
            x => x.RemoveAsync(pollKey),
            Times.Once);
    }

    public void Dispose()
    {
        _memoryCache.Dispose();
        GC.SuppressFinalize(this);
    }
}
