namespace Ilnitsky.Polls.Services.RedisCache;

public interface IRedisCacheService
{
    Task<RedisCacheResult<T>> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T? value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
}
