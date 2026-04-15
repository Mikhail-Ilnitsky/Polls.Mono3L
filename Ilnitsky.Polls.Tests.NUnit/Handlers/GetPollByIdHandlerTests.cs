using System;
using System.Threading.Tasks;

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

namespace Ilnitsky.Polls.Tests.XUnit.Handlers;

public class GetPollByIdHandlerTests
{
    private Mock<IDualCacheService> _cacheMock;
    private MemoryCacheOptionsProvider _memoryOptions;
    private RedisCacheOptionsProvider _redisOptions;
    private ApplicationDbContext _dbContext;

    [SetUp]
    public void Setup()
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

    [Test]
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
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value?.PollId, Is.EqualTo(pollId));
        Assert.That(result.Value?.Name, Is.EqualTo("Марки китайских автомобилей"));
        Assert.That(result.ErrorType, Is.EqualTo(ErrorType.None));
        Assert.That(result.ErrorDetails, Is.Null);
    }

    [Test]
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
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value?.PollId, Is.EqualTo(pollId));
        Assert.That(result.Value?.Name, Is.EqualTo("Марки китайских автомобилей"));
        Assert.That(result.ErrorType, Is.EqualTo(ErrorType.None));
        Assert.That(result.ErrorDetails, Is.Null);

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

    [Test]
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
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorType, Is.EqualTo(ErrorType.EntityNotFound));
        Assert.That(result.Message, Is.EqualTo("Опрос не найден!"));
        Assert.That(result.ErrorDetails, Does.Contain(pollId.ToString()));
    }

    [Test]
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
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorType, Is.EqualTo(ErrorType.EntityNotFound));
        Assert.That(result.Message, Is.EqualTo("Опрос не найден!"));
        Assert.That(result.ErrorDetails, Does.Contain(pollId.ToString()));
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
    }
}
