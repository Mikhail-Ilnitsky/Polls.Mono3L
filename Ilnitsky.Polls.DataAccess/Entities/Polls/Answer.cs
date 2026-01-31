using System;

namespace Ilnitsky.Polls.DataAccess.Entities.Polls;

public class Answer : IEntity
{
    public Guid Id { get; set; }
    public Guid QuestionId { get; set; }

    public string? Text { get; set; }

    public virtual Question? Question { get; set; }
}
