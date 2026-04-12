using System;
using System.Linq;

using Ilnitsky.Polls.BusinessLogic;
using Ilnitsky.Polls.Contracts.Dtos.Polls;
using Ilnitsky.Polls.DataAccess.Entities.Polls;

namespace Ilnitsky.Polls.Tests.XUnit.Unit.Mappings;

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
        Assert.Equal("?", result.Name);
    }

    [Fact]
    public void Poll_ToDto_MapsQuestionsRecursively()
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
        Assert.Equal(pollEntity.Questions.First().Answers.First().Text, result.Questions[0].Answers[0]);
    }

    [Fact]
    public void PollDto_ToEntity_MapsQuestionsRecursively()
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
        Assert.Equal(pollEntity.Questions.First().Answers.First().Text, result.Questions.First().Answers.First().Text);
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("019c1aa8-9bf0-750d-9e6d-832de94b1c13")]
    public void PollDto_ToEntity_CreatesNewIdAndDate_WhenCreateIdIsTrue(string guidString)
    {
        // Arrange
        var guid = Guid.Parse(guidString);
        var dto = new PollDto(guid, DateTime.MinValue, "New Poll", null, true, []);

        // Act
        var result = dto.ToEntity(createId: true);

        // Assert
        Assert.NotEqual(guid, result.Id);
        Assert.NotEqual(DateTime.MinValue, result.DateTime);
        Assert.Equal(dto.Name, result.Name);
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("019c1aa8-9bf0-750d-9e6d-832de94b1c13")]
    public void PollDto_ToEntity_KeepsIdAndDateFromDto_WhenCreateIdIsFalse(string guidString)
    {
        // Arrange
        var guid = Guid.Parse(guidString);
        var dto = new PollDto(guid, DateTime.MinValue, "New Poll", null, true, []);

        // Act
        var result = dto.ToEntity(createId: false);

        // Assert
        Assert.Equal(guid, result.Id);
        Assert.Equal(DateTime.MinValue, result.DateTime);
        Assert.Equal(dto.Name, result.Name);
    }
}
