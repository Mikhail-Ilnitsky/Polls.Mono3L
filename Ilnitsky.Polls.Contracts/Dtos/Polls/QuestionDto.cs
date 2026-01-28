using System;
using System.Collections.Generic;

namespace Ilnitsky.Polls.Contracts.Dtos.Polls;

public record QuestionDto(
    Guid QuestionId,
    string Question,
    bool AllowCustomAnswer,
    bool AllowMultipleChoice,
    int Number,
    string? TargetAnswer,
    int? MatchNextNumber,
    int? DefaultNextNumber,
    List<string> Answers);
