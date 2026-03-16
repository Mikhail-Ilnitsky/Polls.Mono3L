namespace Ilnitsky.Polls.Services.Redis;

public interface IRedisService
{
    Task<RedisServiceResult<T>> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T? value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
}
