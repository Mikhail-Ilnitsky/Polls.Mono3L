using Ilnitsky.Polls.Services.OptionsProviders;
using Ilnitsky.Polls.Services.RedisCache;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Ilnitsky.Polls.Services.DualCache;

public class DualCacheService(
    IMemoryCache memoryCache,
    IRedisCacheService redisCache,
    MemoryCacheOptionsProvider memoryCacheOptions,
    RedisCacheOptionsProvider redisCacheOptions,
    ILogger<RedisCacheService> logger)
        : IDualCacheService
{
    private readonly IMemoryCache _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    private readonly IRedisCacheService _redisCache = redisCache ?? throw new ArgumentNullException(nameof(redisCache));
    private readonly MemoryCacheOptionsProvider _memoryCacheOptions = memoryCacheOptions ?? throw new ArgumentNullException(nameof(memoryCacheOptions));
    private readonly RedisCacheOptionsProvider _redisCacheOptions = redisCacheOptions ?? throw new ArgumentNullException(nameof(redisCacheOptions));
    private readonly ILogger<RedisCacheService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public TimeSpan MaxMemoryExpiration => TimeSpan.FromMinutes(10);

    public async Task<RedisCacheResult<T>> GetAsync<T>(string key)
    {
        if (_memoryCache.TryGetValue(key, out T? value))
        {
            _logger.LogDebug("MemoryCache GET: HIT for Кey={Key}", key);
            return new RedisCacheResult<T>(true, value, true);
        }

        _logger.LogDebug("MemoryCache GET: MISS for Кey={Key}", key);
        return await _redisCache.GetAsync<T>(key);
    }

    public async Task SetAsync<T>(
        string key,
        T? value,
        bool isRedisAvailable,
        TimeSpan? redisExpiration = null,
        TimeSpan? memoryExpiration = null)
    {
        redisExpiration ??= _redisCacheOptions.DefaultExpiration;
        memoryExpiration ??= _memoryCacheOptions.DefaultExpiration;

        if (memoryExpiration > MaxMemoryExpiration)
        {
            memoryExpiration = MaxMemoryExpiration;
        }

        if (isRedisAvailable)
        {
            await _redisCache.SetAsync(key, value, redisExpiration);
        }

        _logger.LogDebug("MemoryCache SET for Key={Key}, Expiration={Expiration}", key, memoryExpiration);
        _memoryCache.Set(key, value, memoryExpiration.Value);
    }

    public async Task RemoveAsync(string key)
    {
        await _redisCache.RemoveAsync(key);
        _memoryCache.Remove(key);
        _logger.LogDebug("MemoryCache REMOVE for Key={Key}", key);
    }
}
