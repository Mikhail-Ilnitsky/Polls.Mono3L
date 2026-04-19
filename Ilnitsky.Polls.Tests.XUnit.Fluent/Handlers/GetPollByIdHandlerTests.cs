using System;
using System.Threading.Tasks;

using FluentAssertions;

using Ilnitsky.Polls.BusinessLogic;
using Ilnitsky.Polls.BusinessLogic.Handlers.Polls;
using Ilnitsky.Polls.Contracts.Dtos;
using Ilnitsky.Polls.Contracts.Dtos.Polls;
using Ilnitsky.Polls.DataAccess;
using Ilnitsky.Polls.Services.DualCache;
using Ilnitsky.Polls.Services.OptionsProviders;
using Ilnitsky.Polls.Services.RedisCache;
using Ilnitsky.Polls.Services.Settings;
using Ilnitsky.Polls.Tests.Shared;

using Microsoft.EntityFrameworkCore;

using Moq;

namespace Ilnitsky.Polls.Tests.XUnit.Fluent.Handlers;

public class GetPollByIdHandlerTests : IDisposable
{
    private Mock<IDualCacheService> _cacheMock;
    private MemoryCacheOptionsProvider _memoryOptions;
    private RedisCacheOptionsProvider _redisOptions;
    private ApplicationDbContext _dbContext;

    public GetPollByIdHandlerTests()
    {
        _cacheMock = new();

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

        // Создаем уникальное имя БД для каждого запуска теста
        var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(dbOptions);
    }

    [Fact]
    public async Task HandleAsync_ReturnsSuccess_WhenPollFoundInCache()
    {
        // Arrange

        var (pollEntity, pollId, pollCacheKey) = TestDbHelper.CreatePoll();
        var pollDto = pollEntity.ToDto();

        // Мокаем в кэше нужное значение для заданного pollCacheKey, и отсутствие значений для других ключей
        _cacheMock
            .Setup(x => x.GetAsync<PollDto>(It.IsAny<string>()))
            .ReturnsAsync(new RedisCacheResult<PollDto>(HasValue: false, Value: null, IsRedisAvailable: true));
        _cacheMock
            .Setup(x => x.GetAsync<PollDto>(pollCacheKey))
            .ReturnsAsync(new RedisCacheResult<PollDto>(HasValue: true, Value: pollDto, IsRedisAvailable: true));

        var handler = new GetPollByIdHandler(_cacheMock.Object, _memoryOptions, _redisOptions, _dbContext);

        // Act
        var result = await handler.HandleAsync(pollId);

        // Assert
        result.Should().BeEquivalentTo(new
        {
            IsSuccess = true,
            ErrorType = ErrorType.None,
            ErrorDetails = (string?)null,
            Value = pollDto
        });
    }

    [Fact]
    public async Task HandleAsync_ReturnsSuccess_WhenPollExistsInDatabaseOnly()
    {
        // Arrange
        var (pollEntity, pollId, pollCacheKey) = TestDbHelper.CreatePoll();

        // Добавляем в БД
        _dbContext.Polls.Add(pollEntity);
        await _dbContext.SaveChangesAsync();

        // Мокаем пустой кэш (имитируем Cache Miss)
        _cacheMock
            .Setup(x => x.GetAsync<PollDto>(It.IsAny<string>()))
            .ReturnsAsync(new RedisCacheResult<PollDto>(HasValue: false, Value: null, IsRedisAvailable: true));

        var handler = new GetPollByIdHandler(_cacheMock.Object, _memoryOptions, _redisOptions, _dbContext);

        // Act
        var result = await handler.HandleAsync(pollId);

        // Assert
        result.Should().BeEquivalentTo(new
        {
            IsSuccess = true,
            ErrorType = ErrorType.None,
            ErrorDetails = (string?)null,
            Value = new
            {
                PollId = pollId,
                Name = "Марки китайских автомобилей"
            }
        });

        // Проверяем, что метод SetAsync был вызван (данные попали в кэш)
        _cacheMock
            .Verify(
                x => x.SetAsync(
                    pollCacheKey,
                    It.IsAny<PollDto>(),
                    true,
                    _redisOptions.PollExpiration,
                    _memoryOptions.PollExpiration),
                Times.Once);    // Метод был вызван ровно 1 раз
    }

    [Fact]
    public async Task HandleAsync_ReturnsNotFound_WhenPollDoesNotExistAnywhere()
    {
        // Arrange
        var pollId = GuidHelper.CreateGuidV7();

        // Мокаем пустой кэш (имитируем Cache Miss)
        _cacheMock
            .Setup(x => x.GetAsync<PollDto>(It.IsAny<string>()))
            .ReturnsAsync(new RedisCacheResult<PollDto>(HasValue: false, Value: null, IsRedisAvailable: true));

        var handler = new GetPollByIdHandler(_cacheMock.Object, _memoryOptions, _redisOptions, _dbContext);

        // Act
        var result = await handler.HandleAsync(pollId);

        // Assert
        result.Should().BeEquivalentTo(new
        {
            IsSuccess = false,
            ErrorType = ErrorType.EntityNotFound,
            Message = "Опрос не найден!",
            Value = (PollDto?)null
        });
        result.ErrorDetails.Should().Contain(pollId.ToString());
    }

    [Fact]
    public async Task HandleAsync_ReturnsNotFound_WhenPollAbsenceIsCached()
    {
        // Arrange
        var (pollEntity, pollId, _) = TestDbHelper.CreatePoll();

        // Добавляем в БД чтобы убедиться, что не будет обращения к БД
        _dbContext.Polls.Add(pollEntity);
        await _dbContext.SaveChangesAsync();

        // Мокаем в кэше возврат значения null, означающего что сущности в нет БД
        _cacheMock
            .Setup(x => x.GetAsync<PollDto>(It.IsAny<string>()))
            .ReturnsAsync(new RedisCacheResult<PollDto>(HasValue: true, Value: null, IsRedisAvailable: true));

        var handler = new GetPollByIdHandler(_cacheMock.Object, _memoryOptions, _redisOptions, _dbContext);

        // Act
        var result = await handler.HandleAsync(pollId);

        // Assert
        result.Should().BeEquivalentTo(new
        {
            IsSuccess = false,
            ErrorType = ErrorType.EntityNotFound,
            Message = "Опрос не найден!",
            Value = (PollDto?)null
        });
        result.ErrorDetails.Should().Contain(pollId.ToString());
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }
}
