namespace Ilnitsky.Polls.Contracts.Dtos;

public record Response<TValue>(
    TValue? Value,
    bool IsSuccess,
    string? Message,
    ErrorType ErrorType)
{
    public static Response<TValue> Success(TValue value)
        => new(value, true, null, ErrorType.None);

    public static Response<TValue> EntityNotFound(string message)
        => new(default, false, message, ErrorType.EntityNotFound);

    public static Response<TValue> IncorrectValue(string message)
        => new(default, false, message, ErrorType.IncorrectValue);

    public static Response<TValue> IncorrectFormat(string message)
        => new(default, false, message, ErrorType.IncorrectFormat);

    public static Response<TValue> Error(string message)
        => new(default, false, message, ErrorType.Error);
}
