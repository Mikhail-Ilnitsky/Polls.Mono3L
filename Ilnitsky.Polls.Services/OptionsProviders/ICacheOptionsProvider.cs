using Microsoft.Extensions.Caching.Distributed;

namespace Ilnitsky.Polls.Services.OptionsProviders;

public interface ICacheOptionsProvider
{
    DistributedCacheEntryOptions DefaultExpiration { get; }
    DistributedCacheEntryOptions ShortExpiration { get; }
}
