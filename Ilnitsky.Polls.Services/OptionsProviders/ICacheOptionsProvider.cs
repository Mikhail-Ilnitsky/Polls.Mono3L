namespace Ilnitsky.Polls.Services.OptionsProviders;

public interface ICacheOptionsProvider
{
    TimeSpan DefaultExpiration { get; }
    TimeSpan PollExpiration { get; }
}
