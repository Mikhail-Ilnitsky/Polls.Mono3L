using System;
using System.Threading.Tasks;

using Ilnitsky.Polls.BusinessLogic;
using Ilnitsky.Polls.BusinessLogic.Handlers.Polls;
using Ilnitsky.Polls.Contracts.Dtos;
using Ilnitsky.Polls.Contracts.Dtos.Polls;
using Ilnitsky.Polls.DataAccess;
using Ilnitsky.Polls.DataAccess.Entities.Polls;
using Ilnitsky.Polls.DbInitialization;
using Ilnitsky.Polls.Services.DualCache;
using Ilnitsky.Polls.Services.OptionsProviders;
using Ilnitsky.Polls.Services.RedisCache;
using Ilnitsky.Polls.Services.Settings;

using Microsoft.EntityFrameworkCore;

using Moq;

namespace Ilnitsky.Polls.Tests.XUnit.Unit.Handlers;

public class GetPollByIdHandlerTests : IDisposable
{
    private readonly Mock<IDualCacheService> _cacheMock = new();
    private readonly MemoryCacheOptionsProvider _memoryOptions;
    private readonly RedisCacheOptionsProvider _redisOptions;
    private readonly ApplicationDbContext _dbContext;

    public GetPollByIdHandlerTests()
    {
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

        // Создаем уникальную БД в памяти для каждого теста
        var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(dbOptions);
    }

    private static (Poll PollEntity, Guid PollId, string PollKey) GetPoll()
    {
        var pollId = DbInitializer.CreateGuidV7();
        var pollKey = $"api_poll_{pollId}";
        var pollEntity = new Poll
        {
            Id = pollId,
            DateTime = DateTime.UtcNow,
            Name = "Марки китайских автомобилей",
            Html = "<img class=\"mb-1 w-100\" src=\"https://infotables.ru/images/avto/logo_auto/logo_china_auto.png\">",
            IsActive = true,
            Questions = [
                new Question
                {
                    Id = pollId,
                    Text = "Какие марки китайских автомобилей вы знаете?",
                    AllowCustomAnswer = false,
                    AllowMultipleChoice = true,
                    Number = 1,
                    TargetAnswer = null,
                    MatchNextNumber = null,
                    DefaultNextNumber = null,
                    Answers = DbInitializer.CreateAnswers([
                        "Brilliance", "BYD", "Changan", "Chery", "Dongfeng",
                        "FAW", "Foton", "GAC", "Geely", "Great Wall",
                        "Hafei", "Haima", "Haval", "Hawtai", "JAC",
                        "Lifan", "Zotye",
                    ]),
                },
            ],
        };

        return (pollEntity, pollId, pollKey);
    }

    [Fact]
    public async Task HandleAsync_ReturnsSuccess_WhenPollFoundInCache()
    {
        // Arrange

        var (pollEntity, pollId, pollCacheKey) = GetPoll();
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
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal(pollId, result.Value?.PollId);
        Assert.Equal("Марки китайских автомобилей", result.Value?.Name);
        Assert.Equal(ErrorType.None, result.ErrorType);
        Assert.Null(result.ErrorDetails);
    }

    [Fact]
    public async Task HandleAsync_ReturnsSuccess_WhenPollExistsInDatabaseOnly()
    {
        // Arrange
        var (pollEntity, pollId, pollCacheKey) = GetPoll();

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
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal(pollId, result.Value?.PollId);
        Assert.Equal("Марки китайских автомобилей", result.Value?.Name);
        Assert.Equal(ErrorType.None, result.ErrorType);
        Assert.Null(result.ErrorDetails);

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
        var pollId = DbInitializer.CreateGuidV7();

        // Мокаем пустой кэш (имитируем Cache Miss)
        _cacheMock
            .Setup(x => x.GetAsync<PollDto>(It.IsAny<string>()))
            .ReturnsAsync(new RedisCacheResult<PollDto>(HasValue: false, Value: null, IsRedisAvailable: true));

        var handler = new GetPollByIdHandler(_cacheMock.Object, _memoryOptions, _redisOptions, _dbContext);

        // Act
        var result = await handler.HandleAsync(pollId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.EntityNotFound, result.ErrorType);
        Assert.Equal("Опрос не найден!", result.Message);
        Assert.Contains(pollId.ToString(), result.ErrorDetails);
    }

    [Fact]
    public async Task HandleAsync_ReturnsNotFound_WhenPollAbsenceIsCached()
    {
        // Arrange
        var (pollEntity, pollId, _) = GetPoll();

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
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.EntityNotFound, result.ErrorType);
        Assert.Equal("Опрос не найден!", result.Message);
        Assert.Contains(pollId.ToString(), result.ErrorDetails);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }
}
