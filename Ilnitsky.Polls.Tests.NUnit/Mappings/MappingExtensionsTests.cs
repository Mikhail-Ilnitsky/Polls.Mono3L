using System;
using System.Collections.Generic;
using System.Linq;

using Ilnitsky.Polls.BusinessLogic;
using Ilnitsky.Polls.Contracts.Dtos.Polls;
using Ilnitsky.Polls.DataAccess.Entities.Polls;
using Ilnitsky.Polls.Tests.Shared;

namespace Ilnitsky.Polls.Tests.NUnit.Mappings;

public class MappingExtensionsTests
{
    [Test]
    public void Poll_ToLinkDto_MapsCorrectWithQuestionsCount()
    {
        // Arrange
        var poll = new Poll
        {
            Id = GuidHelper.CreateGuidV7(),
            Name = "Test Poll"
        };
        int questionsCount = 5;

        // Act
        var result = poll.ToLinkDto(questionsCount);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.PollId, Is.EqualTo(poll.Id));
        Assert.That(result.Name, Is.EqualTo(poll.Name));
        Assert.That(result.QuestionsCount, Is.EqualTo(questionsCount));
    }

    [Test]
    public void Poll_ToLinkDto_UsesQuestionMark_WhenNameIsNull()
    {
        // Arrange
        var poll = new Poll
        {
            Name = null
        };

        // Act
        var result = poll.ToLinkDto(5);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("?"));
    }

    [Test]
    public void Poll_ToDto_UsesQuestionMark_WhenNameIsNull()
    {
        // Arrange
        var poll = new Poll
        {
            Id = Guid.NewGuid(),
            Name = null,
            Questions = []
        };

        // Act
        var result = poll.ToDto();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.PollId, Is.EqualTo(poll.Id));
        Assert.That(result.Name, Is.EqualTo("?"));
        Assert.That(result.Questions, Is.Empty);
    }

    [Test]
    public void Poll_ToDto_MapsCorrect()
    {
        // Arrange
        var (pollEntity, _, _) = TestDbHelper.CreatePoll();

        // Act
        var result = pollEntity.ToDto();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Questions, Has.One.Items);
        Assert.That(result.PollId, Is.EqualTo(pollEntity.Id));
        Assert.That(result.Name, Is.EqualTo(pollEntity.Name));
        Assert.That(result.Html, Is.EqualTo(pollEntity.Html));
        Assert.That(result.DateTime, Is.EqualTo(pollEntity.DateTime));
        Assert.That(result.IsActive, Is.EqualTo(pollEntity.IsActive));

        var resultQuestion = result.Questions[0];
        var pollQuestion = pollEntity.Questions.First();

        Assert.That(resultQuestion.QuestionId, Is.EqualTo(pollQuestion.Id));
        Assert.That(resultQuestion.Question, Is.EqualTo(pollQuestion.Text));
        Assert.That(resultQuestion.AllowCustomAnswer, Is.EqualTo(pollQuestion.AllowCustomAnswer));
        Assert.That(resultQuestion.AllowMultipleChoice, Is.EqualTo(pollQuestion.AllowMultipleChoice));
        Assert.That(resultQuestion.Number, Is.EqualTo(pollQuestion.Number));
        Assert.That(resultQuestion.TargetAnswer, Is.EqualTo(pollQuestion.TargetAnswer));
        Assert.That(resultQuestion.MatchNextNumber, Is.EqualTo(pollQuestion.MatchNextNumber));
        Assert.That(resultQuestion.DefaultNextNumber, Is.EqualTo(pollQuestion.DefaultNextNumber));
        Assert.That(resultQuestion.Answers.Count, Is.EqualTo(pollQuestion.Answers.Count));
        Assert.That(resultQuestion.Answers, Is.EqualTo(pollQuestion.Answers.Select(a => a.Text)));
    }

    [Test]
    public void PollDto_ToEntity_MapsCorrect()
    {
        // Arrange
        var (pollEntity, _, _) = TestDbHelper.CreatePoll();
        var pollDto = pollEntity.ToDto();

        // Act
        var result = pollDto.ToEntity(createId: false);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Questions, Has.One.Items);
        Assert.That(result.Id, Is.EqualTo(pollEntity.Id));
        Assert.That(result.Name, Is.EqualTo(pollEntity.Name));
        Assert.That(result.Html, Is.EqualTo(pollEntity.Html));
        Assert.That(result.DateTime, Is.EqualTo(pollEntity.DateTime));
        Assert.That(result.IsActive, Is.EqualTo(pollEntity.IsActive));

        var resultQuestion = result.Questions.First();
        var pollQuestion = pollEntity.Questions.First();

        Assert.That(resultQuestion.Id, Is.EqualTo(pollQuestion.Id));
        Assert.That(resultQuestion.Text, Is.EqualTo(pollQuestion.Text));
        Assert.That(resultQuestion.AllowCustomAnswer, Is.EqualTo(pollQuestion.AllowCustomAnswer));
        Assert.That(resultQuestion.AllowMultipleChoice, Is.EqualTo(pollQuestion.AllowMultipleChoice));
        Assert.That(resultQuestion.Number, Is.EqualTo(pollQuestion.Number));
        Assert.That(resultQuestion.TargetAnswer, Is.EqualTo(pollQuestion.TargetAnswer));
        Assert.That(resultQuestion.MatchNextNumber, Is.EqualTo(pollQuestion.MatchNextNumber));
        Assert.That(resultQuestion.DefaultNextNumber, Is.EqualTo(pollQuestion.DefaultNextNumber));
        Assert.That(resultQuestion.Answers.Count, Is.EqualTo(pollQuestion.Answers.Count));
        Assert.That(resultQuestion.Answers.Select(a => a.Text), Is.EqualTo(pollQuestion.Answers.Select(a => a.Text)));
    }

    [TestCase("00000000-0000-0000-0000-000000000000")]
    [TestCase("019c1aa8-9bf0-750d-9e6d-832de94b1c13")]
    public void PollDto_ToEntity_CreatesNewIdAndDate_WhenCreateIdIsTrue(Guid guid)
    {
        // Arrange
        var dto = new PollDto(guid, DateTime.MinValue, "New Poll", null, true, []);

        // Act
        var result = dto.ToEntity(createId: true);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.Not.EqualTo(guid));
        Assert.That(result.DateTime, Is.Not.EqualTo(DateTime.MinValue));
        Assert.That(result.Name, Is.EqualTo(dto.Name));
    }

    [TestCase("00000000-0000-0000-0000-000000000000")]
    [TestCase("019c1aa8-9bf0-750d-9e6d-832de94b1c13")]
    public void PollDto_ToEntity_KeepsIdAndDateFromDto_WhenCreateIdIsFalse(Guid guid)
    {
        // Arrange
        var dto = new PollDto(guid, DateTime.MinValue, "New Poll", null, true, []);

        // Act
        var result = dto.ToEntity(createId: false);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(guid));
        Assert.That(result.DateTime, Is.EqualTo(DateTime.MinValue));
        Assert.That(result.Name, Is.EqualTo(dto.Name));
    }

    [TestCase("00000000-0000-0000-0000-000000000000")]
    [TestCase("019c1aa8-9bf0-750d-9e6d-832de94b1c13")]
    public void QuestionDto_ToEntity_CreatesNewIds_WhenCreateIdIsTrue(Guid guid)
    {
        // Arrange
        var dto = new QuestionDto(
            QuestionId: guid,
            Question: "How are you?",
            AllowCustomAnswer: true,
            AllowMultipleChoice: false,
            Number: 1,
            TargetAnswer: null,
            MatchNextNumber: null,
            DefaultNextNumber: null,
            Answers: ["Good", "Bad"]);

        // Act
        var result = dto.ToEntity(createId: true);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.Not.EqualTo(dto.QuestionId));
        Assert.That(result.Answers, Is.All.Matches<Answer>(a => a.Id != Guid.Empty));
        Assert.That(result.Answers.Count, Is.EqualTo(2));
        Assert.That(result.Text, Is.EqualTo(dto.Question));
    }

    [TestCase("00000000-0000-0000-0000-000000000000")]
    [TestCase("019c1aa8-9bf0-750d-9e6d-832de94b1c13")]
    public void QuestionDto_ToEntity_KeepsIdFromDto_WhenCreateIdIsFalse(Guid guid)
    {
        // Arrange
        var dto = new QuestionDto(
            QuestionId: guid,
            Question: "How are you?",
            AllowCustomAnswer: true,
            AllowMultipleChoice: false,
            Number: 1,
            TargetAnswer: null,
            MatchNextNumber: null,
            DefaultNextNumber: null,
            Answers: ["Good", "Bad"]);

        // Act
        var result = dto.ToEntity(createId: false);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(dto.QuestionId));
        Assert.That(result.Answers, Is.All.Matches<Answer>(a => a.Id == Guid.Empty));
        Assert.That(result.Answers.Count, Is.EqualTo(2));
        Assert.That(result.Text, Is.EqualTo(dto.Question));
    }

    [Test]
    public void QuestionDto_ToEntity_MapsCorrect()
    {
        // Arrange
        var (pollEntity, _, _) = TestDbHelper.CreatePoll();
        var questionDto = pollEntity.Questions.First().ToDto();

        // Act
        var result = questionDto.ToEntity(createId: false);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(questionDto.QuestionId));
        Assert.That(result.Text, Is.EqualTo(questionDto.Question));
        Assert.That(result.AllowCustomAnswer, Is.EqualTo(questionDto.AllowCustomAnswer));
        Assert.That(result.AllowMultipleChoice, Is.EqualTo(questionDto.AllowMultipleChoice));
        Assert.That(result.Number, Is.EqualTo(questionDto.Number));
        Assert.That(result.TargetAnswer, Is.EqualTo(questionDto.TargetAnswer));
        Assert.That(result.MatchNextNumber, Is.EqualTo(questionDto.MatchNextNumber));
        Assert.That(result.DefaultNextNumber, Is.EqualTo(questionDto.DefaultNextNumber));
        Assert.That(result.Answers.Count, Is.EqualTo(questionDto.Answers.Count));
        Assert.That(result.Answers.Select(a => a.Text), Is.EqualTo(questionDto.Answers));
    }

    [Test]
    public void Question_ToDto_MapsCorrect()
    {
        // Arrange
        var (pollEntity, _, _) = TestDbHelper.CreatePoll();
        var questionEntity = pollEntity.Questions.First();

        // Act
        var result = questionEntity.ToDto();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.QuestionId, Is.EqualTo(questionEntity.Id));
        Assert.That(result.Question, Is.EqualTo(questionEntity.Text));
        Assert.That(result.AllowCustomAnswer, Is.EqualTo(questionEntity.AllowCustomAnswer));
        Assert.That(result.AllowMultipleChoice, Is.EqualTo(questionEntity.AllowMultipleChoice));
        Assert.That(result.Number, Is.EqualTo(questionEntity.Number));
        Assert.That(result.TargetAnswer, Is.EqualTo(questionEntity.TargetAnswer));
        Assert.That(result.MatchNextNumber, Is.EqualTo(questionEntity.MatchNextNumber));
        Assert.That(result.DefaultNextNumber, Is.EqualTo(questionEntity.DefaultNextNumber));
        Assert.That(result.Answers.Count, Is.EqualTo(questionEntity.Answers.Count));
        Assert.That(result.Answers, Is.EqualTo(questionEntity.Answers.Select(a => a.Text)));
    }

    [Test]
    public void Question_ToDto_UsesQuestionMark_WhenFieldsAreNull()
    {
        // Arrange
        var questionEntity = new Question
        {
            Id = Guid.NewGuid(),
            Text = null,
            Answers = new List<Answer>
            {
                new Answer { Text = null }
            }
        };

        // Act
        var result = questionEntity.ToDto();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Question, Is.EqualTo("?"));
        Assert.That(result.Answers, Has.One.Items);
        Assert.That(result.Answers.First(), Is.EqualTo("?"));
    }
}
