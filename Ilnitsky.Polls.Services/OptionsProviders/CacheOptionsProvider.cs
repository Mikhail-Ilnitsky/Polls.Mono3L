using Ilnitsky.Polls.Services.Settings;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace Ilnitsky.Polls.Services.OptionsProviders;

public class CacheOptionsProvider(IOptions<CacheSettings> settings) : ICacheOptionsProvider
{
    public DistributedCacheEntryOptions DefaultExpiration => new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(settings.Value.DefaultExpirationMinutes)
    };

    public DistributedCacheEntryOptions ShortExpiration => new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(settings.Value.ShortExpirationMinutes)
    };
}
