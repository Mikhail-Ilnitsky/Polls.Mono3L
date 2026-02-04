namespace Ilnitsky.Polls.Contracts.Dtos;

public record BaseResponse(
    bool IsSuccess,
    string? Message,
    ErrorType ErrorType)
{
    public static BaseResponse Success()
        => new(true, null, ErrorType.None);

    public static BaseResponse Success(string message)
        => new(true, message, ErrorType.None);

    public static BaseResponse EntityNotFound(string message)
        => new(false, message, ErrorType.EntityNotFound);

    public static BaseResponse IncorrectValue(string message)
        => new(false, message, ErrorType.IncorrectValue);

    public static BaseResponse IncorrectFormat(string message)
        => new(false, message, ErrorType.IncorrectFormat);

    public static BaseResponse Error(string message)
        => new(false, message, ErrorType.IncorrectValue);
}
