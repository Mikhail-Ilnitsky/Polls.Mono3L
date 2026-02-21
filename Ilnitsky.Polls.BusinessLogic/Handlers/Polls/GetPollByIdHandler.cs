using System;
using System.Threading.Tasks;

using Ilnitsky.Polls.Contracts.Dtos;
using Ilnitsky.Polls.Contracts.Dtos.Polls;
using Ilnitsky.Polls.DataAccess;

using Microsoft.EntityFrameworkCore;

namespace Ilnitsky.Polls.BusinessLogic.Handlers.Polls;

public class GetPollByIdHandler(ApplicationDbContext dbContext)
{
    private readonly ApplicationDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task<Response<PollDto>> HandleAsync(Guid pollId)
    {
        var pollEntity = await _dbContext.Polls
            .AsNoTracking()
            .Include(p => p.Questions)
                .ThenInclude(q => q.Answers)
            .AsSingleQuery()
            .FirstOrDefaultAsync(p => p.Id == pollId);

        if (pollEntity is null)
        {
            return Response<PollDto>.EntityNotFound("Опрос не найден!", $"Нет опроса с Id = {pollId}");
        }

        return Response<PollDto>.Success(pollEntity.ToDto());
    }
}
