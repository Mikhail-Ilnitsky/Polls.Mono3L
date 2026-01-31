using System;
using System.Collections.Generic;

namespace Ilnitsky.Polls.Contracts.Dtos.Answers;

public record CreateRespondentAnswerDto(
    Guid PollId,
    Guid QuestionId,
    List<string> Answers)
{
    public override string ToString()
        => $"{{ PollId: {PollId}; QuestionId: {QuestionId}; Answers: [{string.Join(",", Answers)}]; }}";
}
