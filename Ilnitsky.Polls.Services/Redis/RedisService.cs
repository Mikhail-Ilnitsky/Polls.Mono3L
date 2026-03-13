using System.Text.Json;

using Ilnitsky.Polls.Services.OptionsProviders;

using StackExchange.Redis;

namespace Ilnitsky.Polls.Services.Redis;

public class RedisService(
    ICacheOptionsProvider cacheOptions,
    IConnectionMultiplexer connectionMultiplexer)
        : IRedisService
{
    private readonly IDatabase _db = connectionMultiplexer.GetDatabase();
    private readonly ICacheOptionsProvider _cacheOptions = cacheOptions ?? throw new ArgumentNullException(nameof(cacheOptions));

    public async Task SetAsync<T>(string key, T? value, TimeSpan? expiration = null)
    {
        expiration ??= _cacheOptions.DefaultExpiration;

        var valueString = value is null
            ? "ABSENT"
            : JsonSerializer.Serialize(value);

        await _db.StringSetAsync(key, valueString, expiration);
    }

    public async Task<(bool HasValue, T? Value)> GetAsync<T>(string key)
    {
        var redisValue = await _db.StringGetAsync(key);

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

    public async Task RemoveAsync(string key)
    {
        await _db.KeyDeleteAsync(key);
    }
}
