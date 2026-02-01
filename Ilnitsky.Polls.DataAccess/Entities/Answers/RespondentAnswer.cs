using Ilnitsky.Polls.DataAccess.Entities.Polls;
using System;

namespace Ilnitsky.Polls.DataAccess.Entities.Answers;

public class RespondentAnswer : IEntity
{
    public Guid Id { get; set; }
    public Guid QuestionId { get; set; }
    public Guid PollId { get; set; }
    public Guid RespondentSessionId { get; set; }
    public Guid RespondentId { get; set; }

    public DateTime DateTime { get; set; }
    public string? Text { get; set; }
    public Guid? MultipleAnswersId { get; set; }
    public int? MultipleAnswersCount { get; set; }

    public virtual Question? Question { get; set; }
    public virtual Poll? Poll { get; set; }
    public virtual RespondentSession? RespondentSession { get; set; }
    public virtual Respondent? Respondent { get; set; }
}
