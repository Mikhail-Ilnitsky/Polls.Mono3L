namespace Ilnitsky.Polls.Contracts.Dtos;

public enum ErrorType
{
    None = 0,

    EntityNotFound = 1,
    IncorrectValue = 2,
    IncorrectFormat = 3,

    Error = 255,
}
