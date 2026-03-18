namespace Ilnitsky.Polls.Services.Settings;

public class MemoryCacheSettings
{
    public int DefaultExpirationSeconds { get; set; }
    public int PollExpirationSeconds { get; set; }
}
