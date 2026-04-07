using System;
using System.Linq;

using Ilnitsky.Polls.DataAccess.Entities.Polls;
using Ilnitsky.Polls.DbInitialization;

namespace Ilnitsky.Polls.Tests.XUnit.Unit.Handlers;

public static class TestDbHelper
{
    public static (Poll PollEntity, Guid PollId, string PollKey) CreatePoll()
    {
        var pollEntity = DbInitializer.CreatePolls().First();

        var pollId = pollEntity.Id;
        var pollKey = $"api_poll_{pollId}";

        return (pollEntity, pollId, pollKey);
    }
}
