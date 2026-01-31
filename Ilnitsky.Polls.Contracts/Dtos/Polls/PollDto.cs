using System;
using System.Collections.Generic;

namespace Ilnitsky.Polls.Contracts.Dtos.Polls;

public record PollDto(
    Guid PollId,
    DateTime DateTime,
    string Name,
    string? Html,
    bool IsActive,
    List<QuestionDto> Questions);
