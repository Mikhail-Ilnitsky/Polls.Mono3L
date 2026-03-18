using Ilnitsky.Polls.Services.Settings;

using Microsoft.Extensions.Options;

namespace Ilnitsky.Polls.Services.OptionsProviders;

public class RedisCacheOptionsProvider(IOptions<RedisCacheSettings> settings) : ICacheOptionsProvider
{
    public TimeSpan DefaultExpiration => TimeSpan.FromMinutes(settings.Value.DefaultExpirationMinutes);
    public TimeSpan PollExpiration => TimeSpan.FromMinutes(settings.Value.PollExpirationMinutes);
}
