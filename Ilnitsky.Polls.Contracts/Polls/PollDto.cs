using System;
using System.Collections.Generic;

namespace Ilnitsky.Polls.Contracts.Polls;

public record PollDto(
    Guid PollId,
    string Name,
    string Html,
    bool IsActive,
    List<QuestionDto> Questions);
