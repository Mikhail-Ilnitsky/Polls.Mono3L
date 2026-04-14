using System;
using System.Collections.Generic;
using System.Linq;

using Ilnitsky.Polls.BusinessLogic;
using Ilnitsky.Polls.Contracts.Dtos.Polls;
using Ilnitsky.Polls.DataAccess.Entities.Polls;
using Ilnitsky.Polls.Tests.Shared;

namespace Ilnitsky.Polls.Tests.XUnit.Mappings;

public class MappingExtensionsTests
{
    [Fact]
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
        Assert.NotNull(result);
        Assert.Equal(poll.Id, result.PollId);
        Assert.Equal(poll.Name, result.Name);
        Assert.Equal(questionsCount, result.QuestionsCount);
    }

    [Fact]
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
        Assert.NotNull(result);
        Assert.Equal("?", result.Name);
    }

    [Fact]
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
        Assert.NotNull(result);
        Assert.Equal(poll.Id, result.PollId);
        Assert.Equal("?", result.Name);
        Assert.Empty(result.Questions);
    }

    [Fact]
    public void Poll_ToDto_MapsCorrect()
    {
        // Arrange
        var (pollEntity, _, _) = TestDbHelper.CreatePoll();

        // Act
        var result = pollEntity.ToDto();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Questions);
        Assert.Equal(pollEntity.Id, result.PollId);
        Assert.Equal(pollEntity.Name, result.Name);
        Assert.Equal(pollEntity.Html, result.Html);
        Assert.Equal(pollEntity.DateTime, result.DateTime);
        Assert.Equal(pollEntity.IsActive, result.IsActive);
        Assert.Equal(pollEntity.Questions.First().Id, result.Questions[0].QuestionId);
        Assert.Equal(pollEntity.Questions.First().Text, result.Questions[0].Question);
        Assert.Equal(pollEntity.Questions.First().AllowCustomAnswer, result.Questions[0].AllowCustomAnswer);
        Assert.Equal(pollEntity.Questions.First().AllowMultipleChoice, result.Questions[0].AllowMultipleChoice);
        Assert.Equal(pollEntity.Questions.First().Number, result.Questions[0].Number);
        Assert.Equal(pollEntity.Questions.First().TargetAnswer, result.Questions[0].TargetAnswer);
        Assert.Equal(pollEntity.Questions.First().MatchNextNumber, result.Questions[0].MatchNextNumber);
        Assert.Equal(pollEntity.Questions.First().DefaultNextNumber, result.Questions[0].DefaultNextNumber);
        Assert.Equal(pollEntity.Questions.First().Answers.Count, result.Questions[0].Answers.Count);
        Assert.Equal(pollEntity.Questions.First().Answers.Select(a => a.Text), result.Questions[0].Answers);
    }

    [Fact]
    public void PollDto_ToEntity_MapsCorrect()
    {
        // Arrange
        var (pollEntity, _, _) = TestDbHelper.CreatePoll();
        var pollDto = pollEntity.ToDto();

        // Act
        var result = pollDto.ToEntity(createId: false);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Questions);
        Assert.Equal(pollEntity.Id, result.Id);
        Assert.Equal(pollEntity.Name, result.Name);
        Assert.Equal(pollEntity.Html, result.Html);
        Assert.Equal(pollEntity.DateTime, result.DateTime);
        Assert.Equal(pollEntity.IsActive, result.IsActive);
        Assert.Equal(pollEntity.Questions.First().Id, result.Questions.First().Id);
        Assert.Equal(pollEntity.Questions.First().Text, result.Questions.First().Text);
        Assert.Equal(pollEntity.Questions.First().AllowCustomAnswer, result.Questions.First().AllowCustomAnswer);
        Assert.Equal(pollEntity.Questions.First().AllowMultipleChoice, result.Questions.First().AllowMultipleChoice);
        Assert.Equal(pollEntity.Questions.First().Number, result.Questions.First().Number);
        Assert.Equal(pollEntity.Questions.First().TargetAnswer, result.Questions.First().TargetAnswer);
        Assert.Equal(pollEntity.Questions.First().MatchNextNumber, result.Questions.First().MatchNextNumber);
        Assert.Equal(pollEntity.Questions.First().DefaultNextNumber, result.Questions.First().DefaultNextNumber);
        Assert.Equal(pollEntity.Questions.First().Answers.Count, result.Questions.First().Answers.Count);
        Assert.Equal(pollEntity.Questions.First().Answers.Select(a => a.Text), result.Questions.First().Answers.Select(a => a.Text));
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("019c1aa8-9bf0-750d-9e6d-832de94b1c13")]
    public void PollDto_ToEntity_CreatesNewIdAndDate_WhenCreateIdIsTrue(Guid guid)
    {
        // Arrange
        var dto = new PollDto(guid, DateTime.MinValue, "New Poll", null, true, []);

        // Act
        var result = dto.ToEntity(createId: true);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(guid, result.Id);
        Assert.NotEqual(DateTime.MinValue, result.DateTime);
        Assert.Equal(dto.Name, result.Name);
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("019c1aa8-9bf0-750d-9e6d-832de94b1c13")]
    public void PollDto_ToEntity_KeepsIdAndDateFromDto_WhenCreateIdIsFalse(Guid guid)
    {
        // Arrange
        var dto = new PollDto(guid, DateTime.MinValue, "New Poll", null, true, []);

        // Act
        var result = dto.ToEntity(createId: false);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(guid, result.Id);
        Assert.Equal(DateTime.MinValue, result.DateTime);
        Assert.Equal(dto.Name, result.Name);
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("019c1aa8-9bf0-750d-9e6d-832de94b1c13")]
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
        Assert.NotNull(result);
        Assert.NotEqual(dto.QuestionId, result.Id);
        Assert.All(result.Answers, a => Assert.NotEqual(Guid.Empty, a.Id));
        Assert.Equal(2, result.Answers.Count);
        Assert.Equal(dto.Question, result.Text);
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("019c1aa8-9bf0-750d-9e6d-832de94b1c13")]
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
        Assert.NotNull(result);
        Assert.Equal(dto.QuestionId, result.Id);
        Assert.All(result.Answers, a => Assert.Equal(Guid.Empty, a.Id));
        Assert.Equal(2, result.Answers.Count);
        Assert.Equal(dto.Question, result.Text);
    }

    [Fact]
    public void QuestionDto_ToEntity_MapsCorrect()
    {
        // Arrange
        var (pollEntity, _, _) = TestDbHelper.CreatePoll();
        var questionDto = pollEntity.Questions.First().ToDto();

        // Act
        var result = questionDto.ToEntity(createId: false);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(questionDto.QuestionId, result.Id);
        Assert.Equal(questionDto.Question, result.Text);
        Assert.Equal(questionDto.AllowCustomAnswer, result.AllowCustomAnswer);
        Assert.Equal(questionDto.AllowMultipleChoice, result.AllowMultipleChoice);
        Assert.Equal(questionDto.Number, result.Number);
        Assert.Equal(questionDto.TargetAnswer, result.TargetAnswer);
        Assert.Equal(questionDto.MatchNextNumber, result.MatchNextNumber);
        Assert.Equal(questionDto.DefaultNextNumber, result.DefaultNextNumber);
        Assert.Equal(questionDto.Answers.Count, result.Answers.Count);
        Assert.Equal(questionDto.Answers, result.Answers.Select(a => a.Text));
    }

    [Fact]
    public void Question_ToDto_MapsCorrect()
    {
        // Arrange
        var (pollEntity, _, _) = TestDbHelper.CreatePoll();
        var questionEntity = pollEntity.Questions.First();

        // Act
        var result = questionEntity.ToDto();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(questionEntity.Id, result.QuestionId);
        Assert.Equal(questionEntity.Text, result.Question);
        Assert.Equal(questionEntity.AllowCustomAnswer, result.AllowCustomAnswer);
        Assert.Equal(questionEntity.AllowMultipleChoice, result.AllowMultipleChoice);
        Assert.Equal(questionEntity.Number, result.Number);
        Assert.Equal(questionEntity.TargetAnswer, result.TargetAnswer);
        Assert.Equal(questionEntity.MatchNextNumber, result.MatchNextNumber);
        Assert.Equal(questionEntity.DefaultNextNumber, result.DefaultNextNumber);
        Assert.Equal(questionEntity.Answers.Count, result.Answers.Count);
        Assert.Equal(questionEntity.Answers.Select(a => a.Text), result.Answers);
    }

    [Fact]
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
        Assert.NotNull(result);
        Assert.Equal("?", result.Question);
        Assert.Single(result.Answers);
        Assert.Equal("?", result.Answers.First());
    }
}
