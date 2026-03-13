using System.Text.Json;

using Ilnitsky.Polls.Services.OptionsProviders;

using Microsoft.Extensions.Logging;

using StackExchange.Redis;

namespace Ilnitsky.Polls.Services.Redis;

public class RedisService(
    ICacheOptionsProvider cacheOptions,
    IConnectionMultiplexer connectionMultiplexer,
    ILogger<RedisService> logger)
        : IRedisService
{
    private readonly IDatabase _db = connectionMultiplexer?.GetDatabase() ?? throw new ArgumentNullException(nameof(connectionMultiplexer));
    private readonly ICacheOptionsProvider _cacheOptions = cacheOptions ?? throw new ArgumentNullException(nameof(cacheOptions));
    private readonly ILogger<RedisService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task SetAsync<T>(string key, T? value, TimeSpan? expiration = null)
    {
        try
        {
            expiration ??= _cacheOptions.DefaultExpiration;

            var valueString = value is null
                ? "ABSENT"
                : JsonSerializer.Serialize(value);

            var isSetted = await _db.StringSetAsync(key, valueString, expiration);

            _logger.LogDebug("Redis-cache SET: {IsSetted} for Key={Key}, Expiration={Expiration}", isSetted, key, expiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting Redis-cache for key={Key}", key);
        }
    }

    public async Task<(bool HasValue, T? Value)> GetAsync<T>(string key)
    {
        try
        {
            var redisValue = await _db.StringGetAsync(key);

            _logger.LogDebug(
                "Redis-сache GET: {Result} for Кey={Key}",
                redisValue.IsNull ? "MISS" : "HIT",
                key);

            if (redisValue.IsNull)
            {
                return (false, default);
            }
            if (redisValue == "ABSENT")
            {
                return (true, default);
            }

            return (true, JsonSerializer.Deserialize<T>(redisValue!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting from Redis-cache for Key={Key}", key);
            return (false, default);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            var isDeleted = await _db.KeyDeleteAsync(key);
            _logger.LogDebug("Redis-сache REMOVE: {IsDeleted} for Key={Key}", isDeleted, key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing from Redis-cache for Key={Key}", key);
        }
    }
}
