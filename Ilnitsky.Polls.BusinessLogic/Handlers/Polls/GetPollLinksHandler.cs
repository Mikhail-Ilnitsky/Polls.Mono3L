using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Ilnitsky.Polls.Contracts.Dtos.Polls;
using Ilnitsky.Polls.DataAccess;

using Microsoft.EntityFrameworkCore;

namespace Ilnitsky.Polls.BusinessLogic.Handlers.Polls;

public class GetPollLinksHandler(ApplicationDbContext dbContext)
{
    private readonly ApplicationDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task<List<PollLinkDto>> HandleAsync(int offset, int limit)
    {
        var polls = (await _dbContext.Polls
                .Include(p => p.Questions)
                    .ThenInclude(q => q.Answers)
                .AsSingleQuery()
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.DateTime)
                .Skip(offset)
                .Take(limit)
                .ToArrayAsync())
                .Select(p => p.ToLinkDto())
                .ToList();

        return polls;
    }
}
