using System;
using System.Collections.Generic;

namespace Ilnitsky.Polls.DataAccess.Entities.Answers;

public class Respondent : IEntity
{
    public Guid Id { get; set; }
    public DateTime DateTime { get; set; }

    public virtual ICollection<RespondentSession> RespondentSessions { get; set; } = new List<RespondentSession>();
    public virtual ICollection<RespondentAnswer> RespondentAnswers { get; set; } = new List<RespondentAnswer>();
}
