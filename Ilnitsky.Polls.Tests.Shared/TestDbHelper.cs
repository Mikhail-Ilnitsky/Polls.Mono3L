using System;
using System.Collections.Generic;
using System.Linq;

using Ilnitsky.Polls.BusinessLogic;
using Ilnitsky.Polls.Contracts.Dtos.Polls;
using Ilnitsky.Polls.DataAccess.Entities.Polls;
using Ilnitsky.Polls.DbInitialization;

namespace Ilnitsky.Polls.Tests.Shared;

public static class TestDbHelper
{
    public static (Poll PollEntity, Guid PollId, string PollKey) CreatePoll()
    {
        var pollEntity = DbInitializer.CreatePolls().First();

        var pollId = pollEntity.Id;
        var pollKey = $"api_poll_{pollId}";

        return (pollEntity, pollId, pollKey);
    }

    public static PollDto CreatePollDto()
        => DbInitializer.CreatePolls().First().ToDto();

    public static List<Poll> CreatePollsList(int count)
        => DbInitializer.CreatePolls().Take(count).ToList();

    public static List<PollLinkDto> CreatePollLinkDtosList(int count)
        => DbInitializer.CreatePolls().Select(p => p.ToLinkDto()).Take(count).ToList();
}
