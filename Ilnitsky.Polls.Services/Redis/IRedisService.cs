namespace Ilnitsky.Polls.Services.Redis;

public interface IRedisService
{
    Task SetAsync<T>(string key, T? value, TimeSpan? expiration = null);
    Task<(bool HasValue, T? Value)> GetAsync<T>(string key);
    Task RemoveAsync(string key);
}
