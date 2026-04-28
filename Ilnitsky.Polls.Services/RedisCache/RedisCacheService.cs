using System.Text.Json;

using Ilnitsky.Polls.Services.OptionsProviders;

using Microsoft.Extensions.Logging;

using Polly.Registry;

using StackExchange.Redis;

namespace Ilnitsky.Polls.Services.RedisCache;

public class RedisCacheService(
    ResiliencePipelineProvider<string> pipelineProvider,
    RedisCacheOptionsProvider cacheOptionsProvider,
    IConnectionMultiplexer connectionMultiplexer,
    ILogger<RedisCacheService> logger)
        : IRedisCacheService
{
    private readonly ResiliencePipelineProvider<string> _provider = pipelineProvider ?? throw new ArgumentNullException(nameof(pipelineProvider));
    private readonly IDatabase _db = connectionMultiplexer?.GetDatabase() ?? throw new ArgumentNullException(nameof(connectionMultiplexer));
    private readonly RedisCacheOptionsProvider _cacheOptionsProvider = cacheOptionsProvider ?? throw new ArgumentNullException(nameof(cacheOptionsProvider));
    private readonly ILogger<RedisCacheService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<RedisCacheResult<T>> GetAsync<T>(string key)
    {
        var result = await _provider
            .GetPipeline<object>("redis-get")
            .ExecuteAsync(async token =>
            {
                var redisValue = await _db.StringGetAsync(key);

                _logger.LogDebug(
                    "Redis GET: {Result} for Кey={Key}",
                    redisValue.IsNull ? "MISS" : "HIT",
                    key);

                if (redisValue.IsNull)
                {
                    return (object)new RedisCacheResult<T>(false, default, true);
                }
                if (redisValue == "ABSENT")
                {
                    return (object)new RedisCacheResult<T>(true, default, true);
                }

                return (object)new RedisCacheResult<T>(true, JsonSerializer.Deserialize<T>(redisValue!), true);
            });

        if (result is RedisCacheResult<T> genericResult)
        {
            return genericResult;
        }
        if (result is RedisCacheResult baseResult)
        {
            return new RedisCacheResult<T>(false, default, baseResult.IsRedisAvailable);
        }

        return new RedisCacheResult<T>(false, default, false);
    }

    public async Task SetAsync<T>(string key, T? value, TimeSpan? expiration = null)
    {
        expiration ??= _cacheOptionsProvider.DefaultExpiration;

        await _provider
            .GetPipeline<object>("redis-set")
            .ExecuteAsync(async token =>
            {
                var valueString = value is null
                    ? "ABSENT"
                    : JsonSerializer.Serialize(value);

                var isSetted = await _db.StringSetAsync(key, valueString, expiration);

                _logger.LogDebug("Redis SET: {IsSetted} for Key={Key}, Expiration={Expiration}", isSetted, key, expiration);
                return new RedisCacheResult(true);
            });
    }

    public async Task RemoveAsync(string key)
    {
        await _provider
            .GetPipeline<object>("redis-remove")
            .ExecuteAsync(async token =>
            {
                var isDeleted = await _db.KeyDeleteAsync(key);
                _logger.LogDebug("Redis REMOVE: {IsDeleted} for Key={Key}", isDeleted, key);
                return new RedisCacheResult(true);
            });
    }
}
