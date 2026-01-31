using Ilnitsky.Polls.DataAccess.Entities.Answers;
using System;
using System.Collections.Generic;

namespace Ilnitsky.Polls.DataAccess.Entities.Polls;

public class Question : IEntity
{
    public Guid Id { get; set; }
    public string? Text { get; set; }
    public bool AllowCustomAnswer { get; set; }
    public bool AllowMultipleChoice { get; set; }
    public int Number { get; set; }
    public string? TargetAnswer { get; set; }
    public int? MatchNextNumber { get; set; }
    public int? DefaultNextNumber { get; set; }

    public Guid PollId { get; set; }
    public virtual Poll? Poll { get; set; }

    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();
    public virtual ICollection<RespondentAnswer> RespondentAnswers { get; set; } = new List<RespondentAnswer>();
}
