using System;
using System.Collections.Generic;

namespace Ilnitsky.Polls.Contracts.Answers;

public record RespondentAnswerDto(
    Guid PollId,
    Guid QuestionId,
    List<string> Answers)
{
    public override string ToString()
        => $"{{ PollId: {PollId}; QuestionId: {QuestionId}; Answers: [{string.Join(",", Answers)}]; }}";
}
