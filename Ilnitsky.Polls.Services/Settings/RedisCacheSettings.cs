namespace Ilnitsky.Polls.Services.Settings;

public class RedisCacheSettings
{
    public int DefaultExpirationMinutes { get; set; }
    public int PollExpirationMinutes { get; set; }
}
