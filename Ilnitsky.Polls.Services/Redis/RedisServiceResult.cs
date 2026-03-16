namespace Ilnitsky.Polls.Services.Redis;

public record RedisServiceResult(bool IsAvailable);

public record RedisServiceResult<T>(
    bool HasValue,
    T? Value,
    bool IsAvailable)
        : RedisServiceResult(IsAvailable);

