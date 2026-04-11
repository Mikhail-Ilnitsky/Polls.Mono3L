using System;
using System.Threading.Tasks;

using Ilnitsky.Polls.BusinessLogic;
using Ilnitsky.Polls.Contracts.Dtos.Polls;
using Ilnitsky.Polls.Services.DualCache;
using Ilnitsky.Polls.Services.OptionsProviders;
using Ilnitsky.Polls.Services.RedisCache;
using Ilnitsky.Polls.Services.Settings;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using Moq;

namespace Ilnitsky.Polls.Tests.XUnit.Unit.Services;

public class DualCacheServiceTests
{
    private readonly MemoryCache _memoryCache;
    private readonly Mock<IRedisCacheService> _redisCacheMock;
    private readonly Mock<ILogger<RedisCacheService>> _loggerMock;
    private readonly MemoryCacheOptionsProvider _memoryOptions;
    private readonly RedisCacheOptionsProvider _redisOptions;

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

        _memoryOptions = new(memoryCacheOptions);
        _redisOptions = new(redisCacheOptions);

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
        Assert.NotNull(result);
        Assert.True(result.IsRedisAvailable);
        Assert.True(result.HasValue);
        Assert.NotNull(result.Value);
        Assert.Equal(pollDto.PollId, result.Value.PollId);
        Assert.Equal(pollDto.Name, result.Value.Name);
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
        Assert.NotNull(result);
        Assert.True(result.IsRedisAvailable);
        Assert.True(result.HasValue);
        Assert.NotNull(result.Value);
        Assert.Equal(pollDto.PollId, result.Value.PollId);
        Assert.Equal(pollDto.Name, result.Value.Name);
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
        Assert.NotNull(result);
        Assert.True(result.IsRedisAvailable);
        Assert.False(result.HasValue);
        Assert.Null(result.Value);
        // Проверяем, что Redis вызывался один раз
        _redisCacheMock.Verify(
            x => x.GetAsync<PollDto>(pollKey),
            Times.Once);
    }

    [Fact]
    public async Task SetAsync_SavesToCache_WhenExpirationIsNormal()
    {
        // Arrange
        var service = CreateService();
        var (pollEntity, _, pollKey) = TestDbHelper.CreatePoll();
        var pollDto = pollEntity.ToDto();
        var longExpiration = TimeSpan.FromMinutes(1);

        // Act
        await service.SetAsync(pollKey, pollDto, true, memoryExpiration: longExpiration);

        // Assert
        var isValue = _memoryCache.TryGetValue<PollDto>(pollKey, out var value);
        Assert.True(isValue);
        Assert.Equal(pollDto.PollId, value?.PollId);
    }

    [Fact]
    public async Task SetAsync_SavesToCache_EvenIfExpirationExceedsMax()
    {
        // Arrange
        var service = CreateService();
        var (pollEntity, _, pollKey) = TestDbHelper.CreatePoll();
        var pollDto = pollEntity.ToDto();
        var longExpiration = TimeSpan.FromHours(1);

        // Act
        await service.SetAsync(pollKey, pollDto, true, memoryExpiration: longExpiration);

        // Assert
        var isValue = _memoryCache.TryGetValue<PollDto>(pollKey, out var value);
        Assert.True(isValue);
        Assert.Equal(pollDto.PollId, value?.PollId);
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
        Assert.False(_memoryCache.TryGetValue(pollKey, out _));
        _redisCacheMock.Verify(
            x => x.RemoveAsync(pollKey),
            Times.Once);
    }
}
