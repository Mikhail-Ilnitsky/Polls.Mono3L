using System;

using Ilnitsky.Polls.Contracts.Providers;
using Ilnitsky.Polls.Contracts.Settings;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace Ilnitsky.Polls.Providers;

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
