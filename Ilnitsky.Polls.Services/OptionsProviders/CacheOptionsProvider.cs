using Ilnitsky.Polls.Services.Settings;

using Microsoft.Extensions.Options;

namespace Ilnitsky.Polls.Services.OptionsProviders;

public class CacheOptionsProvider(IOptions<CacheSettings> settings) : ICacheOptionsProvider
{
    public TimeSpan DefaultExpiration => TimeSpan.FromMinutes(settings.Value.DefaultExpirationMinutes);

    public TimeSpan ShortExpiration => TimeSpan.FromMinutes(settings.Value.ShortExpirationMinutes);
}
