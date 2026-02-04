using Ilnitsky.Polls.Contracts.Dtos;
using Ilnitsky.Polls.Contracts.Dtos.Polls;
using Ilnitsky.Polls.DataAccess;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Ilnitsky.Polls.BusinessLogic.Handlers.Polls;

public class GetPollByIdHandler(ApplicationDbContext dbContext)
{
    private readonly ApplicationDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task<Response<PollDto>> HandleAsync(Guid pollId)
    {
        var pollEntity = await _dbContext.Polls
            .FirstOrDefaultAsync(p => p.Id == pollId);

        if (pollEntity is null)
        {
            return Response<PollDto>.EntityNotFound("Запрошенный опрос не найден!");
        }

        return Response<PollDto>.Success(pollEntity.ToDto());
    }
}
