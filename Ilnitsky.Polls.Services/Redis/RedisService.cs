using System.Text.Json;

using Ilnitsky.Polls.Services.OptionsProviders;

using Microsoft.Extensions.Logging;

using Polly.Registry;

using StackExchange.Redis;

namespace Ilnitsky.Polls.Services.Redis;

public class RedisService(
    ResiliencePipelineProvider<string> pipelineProvider,
    ICacheOptionsProvider cacheOptions,
    IConnectionMultiplexer connectionMultiplexer,
    ILogger<RedisService> logger)
        : IRedisService
{
    private readonly ResiliencePipelineProvider<string> _provider = pipelineProvider ?? throw new ArgumentNullException(nameof(pipelineProvider));
    private readonly IDatabase _db = connectionMultiplexer?.GetDatabase() ?? throw new ArgumentNullException(nameof(connectionMultiplexer));
    private readonly ICacheOptionsProvider _cacheOptions = cacheOptions ?? throw new ArgumentNullException(nameof(cacheOptions));
    private readonly ILogger<RedisService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<RedisServiceResult<T>> GetAsync<T>(string key)
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
                    return (object)new RedisServiceResult<T>(false, default, true);
                }
                if (redisValue == "ABSENT")
                {
                    return (object)new RedisServiceResult<T>(true, default, true);
                }

                return (object)new RedisServiceResult<T>(true, JsonSerializer.Deserialize<T>(redisValue!), true);
            });

        if (result is RedisServiceResult<T> genericResult)
        {
            return genericResult;
        }
        if (result is RedisServiceResult baseResult)
        {
            return new RedisServiceResult<T>(true, default, baseResult.IsAvailable);
        }

        return new RedisServiceResult<T>(true, default, false);
    }

    public async Task SetAsync<T>(string key, T? value, TimeSpan? expiration = null)
    {
        expiration ??= _cacheOptions.DefaultExpiration;

        await _provider
            .GetPipeline<object>("redis-set")
            .ExecuteAsync(async token =>
            {
                var valueString = value is null
                    ? "ABSENT"
                    : JsonSerializer.Serialize(value);

                var isSetted = await _db.StringSetAsync(key, valueString, expiration);

                _logger.LogDebug("Redis SET: {IsSetted} for Key={Key}, Expiration={Expiration}", isSetted, key, expiration);
                return new RedisServiceResult(true);
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
                return new RedisServiceResult(true);
            });
    }
}
