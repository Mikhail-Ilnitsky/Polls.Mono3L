using System;
using System.Collections.Generic;

namespace Ilnitsky.Polls.DataAccess.Entities.Answers;

public class RespondentSession : IEntity
{
    public Guid Id { get; set; }
    public Guid RespondentId { get; set; }

    public DateTime DateTime { get; set; }
    public bool IsMobile { get; set; }
    public string? RemoteIpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? AcceptLanguage { get; set; }
    public string? Platform { get; set; }
    public string? Brand { get; set; }

    public virtual Respondent? Respondent { get; set; }

    public virtual ICollection<RespondentAnswer> RespondentAnswers { get; set; } = [];
}
