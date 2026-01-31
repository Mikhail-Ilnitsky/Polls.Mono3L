using Ilnitsky.Polls.DataAccess.Entities.Answers;
using System;
using System.Collections.Generic;

namespace Ilnitsky.Polls.DataAccess.Entities.Polls;

public class Poll : IEntity
{
    public Guid Id { get; set; }

    public DateTime DateTime { get; set; }
    public string? Name { get; set; }
    public string? Html { get; set; }
    public bool IsActive { get; set; }

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
    public virtual ICollection<RespondentAnswer> RespondentAnswers { get; set; } = new List<RespondentAnswer>();
}
