using System;
using System.Text.Json;
using System.Threading.Tasks;

using FluentAssertions;

using Ilnitsky.Polls.BusinessLogic;
using Ilnitsky.Polls.Contracts.Dtos.Polls;
using Ilnitsky.Polls.Services.OptionsProviders;
using Ilnitsky.Polls.Services.RedisCache;
using Ilnitsky.Polls.Services.Settings;
using Ilnitsky.Polls.Tests.Shared;

using Microsoft.Extensions.Logging;

using Moq;

using Polly;
using Polly.Registry;

using StackExchange.Redis;

namespace Ilnitsky.Polls.Tests.XUnit.Fluent.Services;

public class RedisCacheServiceTests
{
    private Mock<IConnectionMultiplexer> _redisMock;
    private Mock<IDatabase> _dbMock;
    private Mock<ILogger<RedisCacheService>> _loggerMock;
    private RedisCacheOptionsProvider _optionsProvider;
    private Mock<ResiliencePipelineProvider<string>> _pipelineProviderMock;

    public RedisCacheServiceTests()
    {
        _dbMock = new Mock<IDatabase>();
        _redisMock = new Mock<IConnectionMultiplexer>();
        _redisMock
            .Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(_dbMock.Object);

        _loggerMock = new Mock<ILogger<RedisCacheService>>();

        var redisCacheSettings = new RedisCacheSettings
        {
            DefaultExpirationMinutes = 30,
            PollExpirationMinutes = 60
        };
        var redisCacheOptions = Microsoft.Extensions.Options.Options.Create(redisCacheSettings);
        _optionsProvider = new RedisCacheOptionsProvider(redisCacheOptions);

        // Мокаем Polly: возвращаем пустой пайплайн, который просто выполняет код
        _pipelineProviderMock = new Mock<ResiliencePipelineProvider<string>>();
        _pipelineProviderMock
            .Setup(x => x.GetPipeline<object>(It.IsAny<string>()))
            .Returns(ResiliencePipeline<object>.Empty);
    }

    private RedisCacheService CreateService() =>
        new(_pipelineProviderMock.Object, _optionsProvider, _redisMock.Object, _loggerMock.Object);

    [Fact]
    public async Task GetAsync_ReturnsValue_WhenKeyExists()
    {
        // Arrange
        var service = CreateService();
        var (pollEntity, pollId, pollKey) = TestDbHelper.CreatePoll();
        var pollDto = pollEntity.ToDto();
        var pollJson = JsonSerializer.Serialize(pollDto);

        _dbMock
            .Setup(x => x.StringGetAsync(pollKey, It.IsAny<CommandFlags>()))
            .ReturnsAsync(pollJson);

        // Act
        var result = await service.GetAsync<PollDto>(pollKey);

        // Assert
        result.Should().BeEquivalentTo(new
        {
            IsRedisAvailable = true,
            HasValue = true,
            Value = new
            {
                pollDto.Name
            }
        });
    }

    [Fact]
    public async Task GetAsync_ReturnsValueNull_WhenKeyExistsAndValueIsABSENT()
    {
        // Arrange
        var service = CreateService();
        var (_, _, pollKey) = TestDbHelper.CreatePoll();
        var pollJson = "ABSENT";

        _dbMock
            .Setup(x => x.StringGetAsync(pollKey, It.IsAny<CommandFlags>()))
            .ReturnsAsync(pollJson);

        // Act
        var result = await service.GetAsync<PollDto>(pollKey);

        // Assert
        result.Should().BeEquivalentTo(new
        {
            IsRedisAvailable = true,
            HasValue = true,
            Value = (object?)null
        });
    }

    [Fact]
    public async Task GetAsync_ReturnsHasValueFalse_WhenKeyDoesNotExist()
    {
        // Arrange
        var service = CreateService();
        var (_, _, pollKey) = TestDbHelper.CreatePoll();
        _dbMock
            .Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        // Act
        var result = await service.GetAsync<PollDto>(pollKey);

        // Assert
        result.Should().BeEquivalentTo(new
        {
            IsRedisAvailable = true,
            HasValue = false,
            Value = (object?)null
        });
    }

    [Fact]
    public async Task SetAsync_CallsRedisWithCorrectParameters()
    {
        // Arrange
        var service = CreateService();

        var (pollEntity, pollId, pollKey) = TestDbHelper.CreatePoll();
        var pollDto = pollEntity.ToDto();
        var pollJson = JsonSerializer.Serialize(pollDto);

        var customTtl = TimeSpan.FromMinutes(10);

        // Act
        await service.SetAsync(pollKey, pollDto, customTtl);

        // Assert
        _dbMock.Verify(
            x => x.StringSetAsync(
                It.Is<RedisKey>(k => k == pollKey),
                It.Is<RedisValue>(v => v == pollJson),
                It.Is<TimeSpan?>(t => t == customTtl),
                false,
                When.Always,
                CommandFlags.None),
            Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_CallsKeyDelete()
    {
        // Arrange
        var service = CreateService();

        // Act
        await service.RemoveAsync("key_for_delete");

        // Assert
        _dbMock.Verify(
            x => x.KeyDeleteAsync(
                (RedisKey)"key_for_delete",
                CommandFlags.None),
            Times.Once);
    }
}
