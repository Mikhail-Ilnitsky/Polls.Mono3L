namespace Ilnitsky.Polls.Contracts.Dtos;

public interface IErrorInfo
{
    string? Message { get; }
    string? ErrorDetails { get; }
    ErrorType ErrorType { get; }
}
