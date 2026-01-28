using Ilnitsky.Polls.Contracts.Dtos.Polls;
using Ilnitsky.Polls.DataAccess;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ilnitsky.Polls.BusinessLogic.Handlers.Polls;

public class GetPollsHandler(ApplicationDbContext dbContext)
{
    private readonly ApplicationDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task<List<PollDto>> HandleAsync()
    {
        var polls = (await _dbContext.Polls
                .Where(p => p.IsActive)
                .ToArrayAsync())
                .Select(p => p.ToDto())
                .ToList();

        return polls;
    }
}
