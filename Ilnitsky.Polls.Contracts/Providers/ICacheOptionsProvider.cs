using Microsoft.Extensions.Caching.Distributed;

namespace Ilnitsky.Polls.Contracts.Providers;

public interface ICacheOptionsProvider
{
    DistributedCacheEntryOptions DefaultExpiration { get; }
    DistributedCacheEntryOptions ShortExpiration { get; }
}
