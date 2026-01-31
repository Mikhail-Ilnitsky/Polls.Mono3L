using Ilnitsky.Polls.Contracts.Dtos.Answers;
using Ilnitsky.Polls.DataAccess;
using Ilnitsky.Polls.DataAccess.Entities.Answers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ilnitsky.Polls.BusinessLogic.Handlers.Answers;

public class CreateRespondentAnswerHandler(ApplicationDbContext dbContext)
{
    private readonly ApplicationDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task HandleAsync(
        CreateRespondentAnswerDto answerDto,
        Guid RespondentSessionId,
        Guid RespondentId)
    {
        var answers = answerDto
            .Answers
            .Select(a => new RespondentAnswer
            {
                Id = GuidHelper.CreateGuidV7(),
                QuestionId = answerDto.QuestionId,
                PollId = answerDto.PollId,
                RespondentSessionId = RespondentSessionId,
                RespondentId = RespondentId,
                Text = a,
                DateTime = DateTime.UtcNow,
            });

        _dbContext.RespondentAnswers.AddRange(answers);

        await _dbContext.SaveChangesAsync();
    }
}
