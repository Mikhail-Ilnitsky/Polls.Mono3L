using Ilnitsky.Polls.Contracts.Dtos;
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

    public async Task<BaseResponse> HandleAsync(
        CreateRespondentAnswerDto answerDto,
        Guid respondentSessionId,
        Guid respondentId)
    {
        if (answerDto.Answers.Count == 0)
        {
            return BaseResponse.IncorrectValue("Не задан ответ!");
        }
        if (!_dbContext.Respondents.Any(r => r.Id == respondentId))
        {
            return BaseResponse.EntityNotFound("Не найден респондент!");
        }
        if (!_dbContext.RespondentSessions.Any(r => r.Id == respondentSessionId))
        {
            return BaseResponse.EntityNotFound("Не найдена сессия респондента!");
        }
        if (!_dbContext.Polls.Any(r => r.Id == answerDto.PollId))
        {
            return BaseResponse.EntityNotFound("Не найден опрос!");
        }

        var question = _dbContext.Questions.FirstOrDefault(r => r.Id == answerDto.QuestionId);

        if (question is null)
        {
            return BaseResponse.EntityNotFound("Не найден вопрос!");
        }
        if (!question.AllowMultipleChoice && answerDto.Answers.Count > 1)
        {
            return BaseResponse.IncorrectValue("На этот вопрос не должно быть больше одного ответа!");
        }
        if (!question.AllowCustomAnswer && !question.Answers.Any(a => a.Text == answerDto.Answers[0]))
        {
            return BaseResponse.IncorrectValue("На этот вопрос не должно быть произвольного ответа!");
        }

        Guid? multipleAnswersId = answerDto.Answers.Count > 1
                ? GuidHelper.CreateGuidV7()
                : null;
        int? multipleAnswersCount = answerDto.Answers.Count > 1
                ? answerDto.Answers.Count
                : null;

        var answers = answerDto
            .Answers
            .Select(a => new RespondentAnswer
            {
                Id = GuidHelper.CreateGuidV7(),
                QuestionId = answerDto.QuestionId,
                PollId = answerDto.PollId,
                RespondentSessionId = respondentSessionId,
                RespondentId = respondentId,
                Text = a,
                DateTime = DateTime.UtcNow,
                MultipleAnswersId = multipleAnswersId,
                MultipleAnswersCount = multipleAnswersCount,
            });

        _dbContext.RespondentAnswers.AddRange(answers);

        await _dbContext.SaveChangesAsync();

        return BaseResponse.Success("Ответ принят!");
    }
}
