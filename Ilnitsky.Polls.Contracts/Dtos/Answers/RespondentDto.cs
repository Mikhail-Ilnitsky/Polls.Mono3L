using System;

namespace Ilnitsky.Polls.Contracts.Dtos.Answers;

public record RespondentDto(
    Guid RespondentId,
    DateTime DateTime);
