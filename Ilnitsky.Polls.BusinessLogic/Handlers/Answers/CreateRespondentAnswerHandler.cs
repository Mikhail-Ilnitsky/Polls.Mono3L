using System;
using System.Linq;
using System.Threading.Tasks;

using Ilnitsky.Polls.Contracts.Dtos;
using Ilnitsky.Polls.Contracts.Dtos.Answers;
using Ilnitsky.Polls.DataAccess;
using Ilnitsky.Polls.DataAccess.Entities.Answers;

using Microsoft.EntityFrameworkCore;

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
            return BaseResponse.IncorrectValue("Не задан ответ!", "Количество ответов равно 0");
        }
        if (answerDto.Answers.Any(string.IsNullOrWhiteSpace))
        {
            return BaseResponse.IncorrectValue("Не должно быть пустых ответов!", "В качестве ответа передана пустая строка или строка пробелов");
        }
        if (!_dbContext.Respondents.Any(r => r.Id == respondentId))
        {
            return BaseResponse.EntityNotFound("Не найден респондент!", $"Нет респондента с Id = {respondentId}");
        }
        if (!_dbContext.RespondentSessions.Any(r => r.Id == respondentSessionId))
        {
            return BaseResponse.EntityNotFound("Не найдена сессия респондента!", $"Нет сессии с Id = {respondentSessionId}");
        }
        if (!_dbContext.Polls.Any(r => r.Id == answerDto.PollId))
        {
            return BaseResponse.EntityNotFound("Не найден опрос!", $"Нет опроса с Id = {answerDto.PollId}");
        }

        var question = _dbContext.Questions
            .Include(q => q.Answers)
            .AsSingleQuery()
            .FirstOrDefault(r => r.Id == answerDto.QuestionId);

        if (question is null)
        {
            return BaseResponse.EntityNotFound("Не найден вопрос!", $"Нет вопроса с Id = {answerDto.QuestionId}");
        }
        if (!question.AllowMultipleChoice && answerDto.Answers.Count > 1)
        {
            return BaseResponse.IncorrectValue("На этот вопрос не должно быть больше одного ответа!", $"Вопрос позволяет только один ответ, но количество ответов равно {answerDto.Answers.Count}");
        }
        if (!question.AllowCustomAnswer && !question.Answers.Any(a => a.Text == answerDto.Answers[0]))
        {
            return BaseResponse.IncorrectValue("На этот вопрос не должно быть произвольного ответа!", $"Передан непредусмотренный ответ '{answerDto.Answers[0]}'");
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

        return BaseResponse.Created("Ответ принят!");
    }
}
