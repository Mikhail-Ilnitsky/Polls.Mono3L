namespace Ilnitsky.Polls.Contracts.Dtos;

public record BaseResponse(
    bool IsSuccess,
    bool IsCreated,
    string? Message,
    string? ErrorDetails,
    ErrorType ErrorType) : IErrorInfo
{
    public static BaseResponse Created()
        => new(true, true, null, null, ErrorType.None);

    public static BaseResponse Created(string message)
        => new(true, true, message, null, ErrorType.None);

    public static BaseResponse Success()
        => new(true, false, null, null, ErrorType.None);

    public static BaseResponse Success(string message)
        => new(true, false, message, null, ErrorType.None);

    public static BaseResponse EntityNotFound(string message, string details)
        => new(false, false, message, details, ErrorType.EntityNotFound);

    public static BaseResponse IncorrectValue(string message, string details)
        => new(false, false, message, details, ErrorType.IncorrectValue);

    public static BaseResponse IncorrectFormat(string message, string details)
        => new(false, false, message, details, ErrorType.IncorrectFormat);

    public static BaseResponse Error(string message, string details)
        => new(false, false, message, details, ErrorType.IncorrectValue);
}
