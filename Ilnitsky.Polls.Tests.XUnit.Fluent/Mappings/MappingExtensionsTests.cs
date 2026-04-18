using System;
using System.Linq;

using FluentAssertions;

using Ilnitsky.Polls.BusinessLogic;
using Ilnitsky.Polls.Contracts.Dtos.Polls;
using Ilnitsky.Polls.DataAccess.Entities.Polls;
using Ilnitsky.Polls.Tests.Shared;

namespace Ilnitsky.Polls.Tests.XUnit.Fluent.Mappings;

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
        // v1
        result.Should().NotBeNull();
        result.PollId.Should().Be(poll.Id);
        result.Name.Should().Be(poll.Name);
        result.QuestionsCount.Should().Be(questionsCount);

        // v2
        result.Should().BeEquivalentTo(new
        {
            PollId = poll.Id,
            poll.Name,
            QuestionsCount = questionsCount
        });
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
        result.Should().NotBeNull();
        result.Name.Should().Be("?");
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
        // v1
        result.Should().BeEquivalentTo(new
        {
            PollId = poll.Id,
            Name = "?",
            Questions = Array.Empty<QuestionDto>()
        });

        // v2
        result.Should().NotBeNull();
        result.PollId.Should().Be(poll.Id);
        result.Name.Should().Be("?");
        result.Questions.Should().BeEmpty();
    }

    [Fact]
    public void Poll_ToDto_MapsCorrect()
    {
        // Arrange
        var (pollEntity, _, _) = TestDbHelper.CreatePoll();

        // Act
        var result = pollEntity.ToDto();

        // Assert
        // v1
        var pollQuestion = pollEntity.Questions.First();
        result.Should().BeEquivalentTo(new
        {
            PollId = pollEntity.Id,
            pollEntity.Name,
            pollEntity.Html,
            pollEntity.DateTime,
            pollEntity.IsActive,
            Questions = new[]
            {
                new
                {
                    QuestionId = pollQuestion.Id,
                    Question = pollQuestion.Text,
                    pollQuestion.AllowCustomAnswer,
                    pollQuestion.AllowMultipleChoice,
                    pollQuestion.Number,
                    pollQuestion.TargetAnswer,
                    pollQuestion.MatchNextNumber,
                    pollQuestion.DefaultNextNumber,
                    Answers = pollQuestion.Answers.Select(a => a.Text)
                }
            }
        });

        // v2
        result.Should().BeEquivalentTo(
            pollEntity,
            options => options
                .ExcludingMissingMembers() // Игнорируем поля в Entity, которых нет в Dto
                .WithMapping<PollDto>(e => e.Id, dto => dto.PollId)
        );
        result.Questions.First().Should().BeEquivalentTo(
            pollEntity.Questions.First(),
            options => options
                .ExcludingMissingMembers() // Игнорируем поля в Entity, которых нет в Dto
                .WithMapping<QuestionDto>(e => e.Id, dto => dto.QuestionId)
                .WithMapping<QuestionDto>(e => e.Text, dto => dto.Question)
        );
        result.Questions.Should().ContainSingle();
        result.Questions.First().Answers.Should().BeEquivalentTo(
            pollEntity.Questions.First().Answers.Select(a => a.Text)
        );
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
        result.Should().BeEquivalentTo(
            pollEntity,
            options => options.Excluding(ctx => ctx.Path.Contains("Answers") && ctx.Path.EndsWith("Id"))
        );
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
        result.Should().NotBeNull();
        result.Id.Should().NotBe(guid);
        result.DateTime.Should().NotBe(DateTime.MinValue);
        result.Name.Should().Be(dto.Name);
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
        // v1
        result.Should().BeEquivalentTo(new
        {
            Id = guid,
            DateTime = DateTime.MinValue,
            dto.Name
        });

        // v2
        result.Should().NotBeNull();
        result.Id.Should().Be(guid);
        result.DateTime.Should().Be(DateTime.MinValue);
        result.Name.Should().Be(dto.Name);
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
        result.Should().NotBeNull();
        result.Id.Should().NotBe(dto.QuestionId);
        result.Text.Should().Be(dto.Question);
        result.Answers.Should().HaveCount(2);
        result.Answers.Should().OnlyContain(a => a.Id != Guid.Empty);

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
        // v1
        result.Should().BeEquivalentTo(
            new
            {
                Id = dto.QuestionId,
                Text = dto.Question,
                Answers = new[]
                {
                    new
                    {
                        Id = Guid.Empty,
                        Text = "Good"
                    },
                    new
                    {
                        Id = Guid.Empty,
                        Text = "Bad"
                    }
                }
            },
            options => options.WithStrictOrdering()
        );

        // v2
        result.Should().NotBeNull();
        result.Id.Should().Be(dto.QuestionId);
        result.Text.Should().Be(dto.Question);
        result.Answers.Should().HaveCount(2);
        result.Answers.Should().OnlyContain(a => a.Id == Guid.Empty);
    }

    [Fact]
    public void QuestionDto_ToEntity_MapsCorrect()
    {
        // Arrange
        var (pollEntity, _, _) = TestDbHelper.CreatePoll();
        var questionEntity = pollEntity.Questions.First();
        var questionDto = questionEntity.ToDto();

        // Act
        var result = questionDto.ToEntity(createId: false);

        // Assert
        // v1
        result.Should().BeEquivalentTo(
            new
            {
                Id = questionDto.QuestionId,
                Text = questionDto.Question,
                questionDto.AllowCustomAnswer,
                questionDto.AllowMultipleChoice,
                questionDto.Number,
                questionDto.TargetAnswer,
                questionDto.MatchNextNumber,
                questionDto.DefaultNextNumber,
                Answers = questionDto.Answers.Select(a => new { Text = a })
            },
            options => options.WithStrictOrdering()
        );

        // v2
        result.Should().BeEquivalentTo(
            questionEntity,
            options => options
                .WithStrictOrdering()
                .Excluding(ctx => ctx.Path.Contains("Answers") && ctx.Path.EndsWith("Id"))
        );
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
        result.Should().BeEquivalentTo(
            new
            {
                QuestionId = questionEntity.Id,
                Question = questionEntity.Text,
                questionEntity.AllowCustomAnswer,
                questionEntity.AllowMultipleChoice,
                questionEntity.Number,
                questionEntity.TargetAnswer,
                questionEntity.MatchNextNumber,
                questionEntity.DefaultNextNumber,
                Answers = questionEntity.Answers.Select(a => a.Text)
            },
            options => options.WithStrictOrdering()
        );
    }

    [Fact]
    public void Question_ToDto_UsesQuestionMark_WhenFieldsAreNull()
    {
        // Arrange
        var questionEntity = new Question
        {
            Id = Guid.NewGuid(),
            Text = null,
            Answers =
            [
                new Answer { Text = null }
            ]
        };

        // Act
        var result = questionEntity.ToDto();

        // Assert
        result.Should().BeEquivalentTo(new
        {
            Question = "?",
            Answers = (string[])["?"]
        });
    }
}
