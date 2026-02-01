using System;

namespace Ilnitsky.Polls.Contracts.Dtos.Polls;

public record PollLinkDto(
    Guid PollId,
    string Name,
    int QuestionsCount);
