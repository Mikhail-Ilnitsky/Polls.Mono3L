using System;
using System.Linq;

using Ilnitsky.Polls.Contracts.Dtos.Polls;
using Ilnitsky.Polls.DataAccess.Entities.Polls;

namespace Ilnitsky.Polls.BusinessLogic;

public static class MappingExtensions
{
    public static PollLinkDto ToLinkDto(this Poll entity)
        => new(
            entity.Id,
            entity.Name ?? "?",
            entity.Questions.Count);

    public static PollDto ToDto(this Poll entity)
        => new(
            entity.Id,
            entity.DateTime,
            entity.Name ?? "?",
            entity.Html,
            entity.IsActive,
            entity.Questions
                .Select(q => q.ToDto())
                .ToList());

    public static Poll ToEntity(this PollDto dto, bool createId = false)
        => new()
        {
            Id = !createId ? dto.PollId : GuidHelper.CreateGuidV7(),
            DateTime = !createId ? dto.DateTime : DateTime.UtcNow,
            Name = dto.Name,
            Html = dto.Html,
            IsActive = dto.IsActive,
            Questions = dto.Questions
                .Select(q => q.ToEntity(createId))
                .ToList()
        };

    public static QuestionDto ToDto(this Question entity)
        => new(
            entity.Id,
            entity.Text ?? "?",
            entity.AllowCustomAnswer,
            entity.AllowMultipleChoice,
            entity.Number,
            entity.TargetAnswer,
            entity.MatchNextNumber,
            entity.DefaultNextNumber,
            entity.Answers
                .Select(a => a.Text ?? "?")
                .ToList());

    public static Question ToEntity(this QuestionDto dto, bool createId = false)
        => new()
        {
            Id = !createId ? dto.QuestionId : GuidHelper.CreateGuidV7(),
            Text = dto.Question,
            AllowCustomAnswer = dto.AllowCustomAnswer,
            AllowMultipleChoice = dto.AllowMultipleChoice,
            Number = dto.Number,
            TargetAnswer = dto.TargetAnswer,
            MatchNextNumber = dto.MatchNextNumber,
            DefaultNextNumber = dto.DefaultNextNumber,
            Answers = dto.Answers
                .Select(a => new Answer
                {
                    Id = !createId ? Guid.Empty : GuidHelper.CreateGuidV7(),
                    Text = a
                })
                .ToList()
        };
}
