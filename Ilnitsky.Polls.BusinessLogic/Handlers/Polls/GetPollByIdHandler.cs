using System;
using System.Text.Json;
using System.Threading.Tasks;

using Ilnitsky.Polls.Contracts.Dtos;
using Ilnitsky.Polls.Contracts.Dtos.Polls;
using Ilnitsky.Polls.DataAccess;
using Ilnitsky.Polls.Services.OptionsProviders;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace Ilnitsky.Polls.BusinessLogic.Handlers.Polls;

public class GetPollByIdHandler(
    IDistributedCache cache,
    ICacheOptionsProvider cacheOptions,
    ApplicationDbContext dbContext)
{
    private readonly IDistributedCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private readonly ICacheOptionsProvider _cacheOptions = cacheOptions ?? throw new ArgumentNullException(nameof(cacheOptions));
    private readonly ApplicationDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task<Response<PollDto>> HandleAsync(Guid pollId)
    {
        var pollKey = $"poll_{pollId}";
        var cachedPollString = await _cache.GetStringAsync(pollKey);

        if (cachedPollString == "ABSENT")
        {
            return GetNotFoundResponse(pollId);
        }
        if (cachedPollString is not null)
        {
            return Response<PollDto>.Success(JsonSerializer.Deserialize<PollDto>(cachedPollString)!);
        }

        var pollEntity = await _dbContext.Polls
            .AsNoTracking()
            .Include(p => p.Questions)
                .ThenInclude(q => q.Answers)
            .AsSingleQuery()
            .FirstOrDefaultAsync(p => p.Id == pollId);

        if (pollEntity is null)
        {
            await _cache.SetStringAsync(pollKey, "ABSENT", _cacheOptions.DefaultExpiration);
            return GetNotFoundResponse(pollId);
        }

        var pollDto = pollEntity.ToDto();
        await _cache.SetStringAsync(pollKey, JsonSerializer.Serialize(pollDto), _cacheOptions.DefaultExpiration);

        return Response<PollDto>.Success(pollDto);
    }

    private static Response<PollDto> GetNotFoundResponse(Guid pollId)
        => Response<PollDto>.EntityNotFound("Опрос не найден!", $"Нет опроса с Id = {pollId}");
}
