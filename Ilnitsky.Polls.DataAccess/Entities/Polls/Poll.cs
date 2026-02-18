using System;
using System.Collections.Generic;

using Ilnitsky.Polls.DataAccess.Entities.Answers;

namespace Ilnitsky.Polls.DataAccess.Entities.Polls;

public class Poll : IEntity
{
    public Guid Id { get; set; }

    public DateTime DateTime { get; set; }
    public string? Name { get; set; }
    public string? Html { get; set; }
    public bool IsActive { get; set; }

    public virtual ICollection<Question> Questions { get; set; } = [];
    public virtual ICollection<RespondentAnswer> RespondentAnswers { get; set; } = [];
}
