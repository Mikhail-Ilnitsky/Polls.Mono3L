namespace Ilnitsky.Polls.Contracts.Dtos;

public record Response<TValue>(
    TValue? Value,
    bool IsSuccess,
    string? Message,
    string? ErrorDetails,
    ErrorType ErrorType) : IErrorInfo
{
    public static Response<TValue> Success(TValue value)
        => new(value, true, null, null, ErrorType.None);

    public static Response<TValue> EntityNotFound(string message, string details)
        => new(default, false, message, details, ErrorType.EntityNotFound);

    public static Response<TValue> IncorrectValue(string message, string details)
        => new(default, false, message, details, ErrorType.IncorrectValue);

    public static Response<TValue> IncorrectFormat(string message, string details)
        => new(default, false, message, details, ErrorType.IncorrectFormat);

    public static Response<TValue> Error(string message, string details)
        => new(default, false, message, details, ErrorType.Error);
}
