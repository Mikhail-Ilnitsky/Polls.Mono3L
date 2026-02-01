using Ilnitsky.Polls.Contracts.Dtos.Polls;
using Ilnitsky.Polls.DataAccess;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Ilnitsky.Polls.BusinessLogic.Handlers.Polls;

public class GetPollByIdHandler(ApplicationDbContext dbContext)
{
    private readonly ApplicationDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task<PollDto?> HandleAsync(Guid pollId)
    {
        return (await _dbContext.Polls
            .FirstOrDefaultAsync(p => p.Id == pollId))
            ?.ToDto();
    }
}
