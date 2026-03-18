using Ilnitsky.Polls.Services.Settings;

using Microsoft.Extensions.Options;

namespace Ilnitsky.Polls.Services.OptionsProviders;

public class MemoryCacheOptionsProvider(IOptions<MemoryCacheSettings> settings) : ICacheOptionsProvider
{
    public TimeSpan DefaultExpiration => TimeSpan.FromSeconds(settings.Value.DefaultExpirationSeconds);
    public TimeSpan PollExpiration => TimeSpan.FromSeconds(settings.Value.PollExpirationSeconds);
}

