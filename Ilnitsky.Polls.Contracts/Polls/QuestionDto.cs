using System;
using System.Collections.Generic;

namespace Ilnitsky.Polls.Contracts.Polls;

public record QuestionDto(
    Guid QuestionId,
    string Question,
    bool AllowCustomAnswer,
    bool AllowMultipleChoice,
    int Number,
    string? ConditionAnswer,
    int? IfConditionNextNumber,
    int? IfNotConditionNextNumber,
    List<string> Answers);
