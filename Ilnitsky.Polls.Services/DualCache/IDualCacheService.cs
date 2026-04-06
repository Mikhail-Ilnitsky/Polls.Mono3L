using Ilnitsky.Polls.Services.RedisCache;

namespace Ilnitsky.Polls.Services.DualCache;

public interface IDualCacheService
{
    TimeSpan MaxMemoryExpiration { get; }
    Task<RedisCacheResult<T>> GetAsync<T>(string key);
    Task SetAsync<T>(
        string key,
        T? value,
        bool IsRedisAvailable,
        TimeSpan? redisExpiration = null,
        TimeSpan? memoryExpiration = null);
    Task RemoveAsync(string key);
}
