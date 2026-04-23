using System;
using System.Threading.Tasks;

using Ilnitsky.Polls.Contracts.Dtos;
using Ilnitsky.Polls.Contracts.Dtos.Polls;
using Ilnitsky.Polls.DataAccess;
using Ilnitsky.Polls.Services.DualCache;
using Ilnitsky.Polls.Services.OptionsProviders;

using Microsoft.EntityFrameworkCore;

namespace Ilnitsky.Polls.BusinessLogic.Handlers.Polls;

public interface IGetPollByIdHandler
{
    Task<Response<PollDto>> HandleAsync(Guid pollId);
}

public class GetPollByIdHandler(
    IDualCacheService cacheService,
    MemoryCacheOptionsProvider memoryCacheOptions,
    RedisCacheOptionsProvider redisCacheOptions,
    ApplicationDbContext dbContext)
        : IGetPollByIdHandler
{
    private readonly IDualCacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    private readonly MemoryCacheOptionsProvider _memoryCacheOptions = memoryCacheOptions ?? throw new ArgumentNullException(nameof(memoryCacheOptions));
    private readonly RedisCacheOptionsProvider _redisCacheOptions = redisCacheOptions ?? throw new ArgumentNullException(nameof(redisCacheOptions));
    private readonly ApplicationDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task<Response<PollDto>> HandleAsync(Guid pollId)
    {
        var cacheKey = $"api_poll_{pollId}";
        var cache = await _cacheService.GetAsync<PollDto>(cacheKey);

        if (cache.HasValue)
        {
            if (cache.Value is null)
            {
                return GetNotFoundResponse(pollId);
            }

            return Response<PollDto>.Success(cache.Value);
        }

        var pollEntity = await _dbContext.Polls
            .AsNoTracking()
            .Include(p => p.Questions)
                .ThenInclude(q => q.Answers)
            .AsSingleQuery()
            .FirstOrDefaultAsync(p => p.Id == pollId);

        if (pollEntity is null)
        {
            await _cacheService.SetAsync<PollDto>(
                cacheKey,
                null,
                cache.IsRedisAvailable,
                _redisCacheOptions.PollExpiration,
                _memoryCacheOptions.PollExpiration);
            return GetNotFoundResponse(pollId);
        }

        var pollDto = pollEntity.ToDto();
        await _cacheService.SetAsync(
            cacheKey,
            pollDto,
            cache.IsRedisAvailable,
            _redisCacheOptions.PollExpiration,
            _memoryCacheOptions.PollExpiration);
        return Response<PollDto>.Success(pollDto);
    }

    private static Response<PollDto> GetNotFoundResponse(Guid pollId)
        => Response<PollDto>.EntityNotFound("Опрос не найден!", $"Нет опроса с Id = {pollId}");
}
