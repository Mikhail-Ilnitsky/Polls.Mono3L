using System;
using System.Threading.Tasks;

using Ilnitsky.Polls.Contracts.Dtos;
using Ilnitsky.Polls.Contracts.Dtos.Polls;
using Ilnitsky.Polls.DataAccess;
using Ilnitsky.Polls.Services.OptionsProviders;
using Ilnitsky.Polls.Services.Redis;

using Microsoft.EntityFrameworkCore;

namespace Ilnitsky.Polls.BusinessLogic.Handlers.Polls;

public class GetPollByIdHandler(
    IRedisService redisService,
    ICacheOptionsProvider cacheOptions,
    ApplicationDbContext dbContext)
{
    private readonly IRedisService _redisService = redisService ?? throw new ArgumentNullException(nameof(redisService));
    private readonly ICacheOptionsProvider _cacheOptions = cacheOptions ?? throw new ArgumentNullException(nameof(cacheOptions));
    private readonly ApplicationDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task<Response<PollDto>> HandleAsync(Guid pollId)
    {
        var cacheKey = $"api_poll_{pollId}";
        var redisCache = await _redisService.GetAsync<PollDto>(cacheKey);

        if (redisCache.IsAvailable && redisCache.HasValue)
        {
            if (redisCache.Value is null)
            {
                return GetNotFoundResponse(pollId);
            }

            return Response<PollDto>.Success(redisCache.Value);
        }

        var pollEntity = await _dbContext.Polls
            .AsNoTracking()
            .Include(p => p.Questions)
                .ThenInclude(q => q.Answers)
            .AsSingleQuery()
            .FirstOrDefaultAsync(p => p.Id == pollId);

        if (pollEntity is null)
        {
            if (redisCache.IsAvailable)
            {
                await _redisService.SetAsync<PollDto>(cacheKey, null, _cacheOptions.DefaultExpiration);
            }

            return GetNotFoundResponse(pollId);
        }

        var pollDto = pollEntity.ToDto();

        if (redisCache.IsAvailable)
        {
            await _redisService.SetAsync(cacheKey, pollDto, _cacheOptions.DefaultExpiration);
        }

        return Response<PollDto>.Success(pollDto);
    }

    private static Response<PollDto> GetNotFoundResponse(Guid pollId)
        => Response<PollDto>.EntityNotFound("Опрос не найден!", $"Нет опроса с Id = {pollId}");
}
