namespace Ilnitsky.Polls.Services.RedisCache;

public record RedisCacheResult(bool IsRedisAvailable);

public record RedisServiceResult<T>(
    bool HasValue,
    T? Value,
    bool IsRedisAvailable)
        : RedisCacheResult(IsRedisAvailable);

