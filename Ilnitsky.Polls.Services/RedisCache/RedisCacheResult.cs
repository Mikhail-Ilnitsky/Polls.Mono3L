namespace Ilnitsky.Polls.Services.RedisCache;

public record RedisCacheResult(bool IsRedisAvailable);

public record RedisCacheResult<T>(
    bool HasValue,
    T? Value,
    bool IsRedisAvailable)
        : RedisCacheResult(IsRedisAvailable);

