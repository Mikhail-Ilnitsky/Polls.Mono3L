using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using FluentAssertions;

using Ilnitsky.Polls.BusinessLogic.Handlers.Polls;
using Ilnitsky.Polls.Contracts.Dtos;
using Ilnitsky.Polls.Contracts.Dtos.Polls;
using Ilnitsky.Polls.Controllers;
using Ilnitsky.Polls.Tests.Shared;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Moq;

namespace Ilnitsky.Polls.Tests.NUnit.Fluent.Controllers;

public class PollsControllerTests
{
    [TestCase(null, null)]
    [TestCase(null, -1)]
    [TestCase(-1, null)]
    [TestCase(-1, -1)]
    [TestCase(-10, -10)]
    public async Task GetPollLinksAsync_UsesDefaults_WhenParametersAreNullOrNegative(int? offset, int? limit)
    {
        // Arrange
        var handlerMock = new Mock<IGetPollLinksHandler>();
        var controller = new PollsController();

        // Act
        await controller.GetPollLinksAsync(offset, limit, handlerMock.Object);

        // Assert
        handlerMock.Verify(h => h.HandleAsync(0, 5), Times.Once);
    }

    [TestCase(null, 1)]
    [TestCase(null, 10)]
    [TestCase(-1, 1)]
    [TestCase(-1, 10)]
    [TestCase(-10, 1)]
    [TestCase(-10, 10)]
    public async Task GetPollLinksAsync_UsesDefaults_WhenOffsetIsNullOrNegative(int? offset, int limit)
    {
        // Arrange
        var handlerMock = new Mock<IGetPollLinksHandler>();
        var controller = new PollsController();

        // Act
        await controller.GetPollLinksAsync(offset, limit, handlerMock.Object);

        // Assert
        handlerMock.Verify(h => h.HandleAsync(0, limit), Times.Once);
    }

    [TestCase(0, null)]
    [TestCase(0, 0)]
    [TestCase(0, -1)]
    [TestCase(0, -10)]
    [TestCase(10, null)]
    [TestCase(10, 0)]
    [TestCase(10, -1)]
    [TestCase(10, -10)]
    public async Task GetPollLinksAsync_UsesDefaults_WhenLimitIsNullOrNonPositive(int offset, int? limit)
    {
        // Arrange
        var handlerMock = new Mock<IGetPollLinksHandler>();
        var controller = new PollsController();

        // Act
        await controller.GetPollLinksAsync(offset, limit, handlerMock.Object);

        // Assert
        handlerMock.Verify(h => h.HandleAsync(offset, 5), Times.Once);
    }

    [TestCase(1)]
    [TestCase(10)]
    [TestCase(100)]
    public async Task GetPollLinksAsync_ReturnsSinglePoll_WhenSingleActivePollExists(int limit)
    {
        // Arrange
        var expectedData = TestDbHelper.CreatePollLinkDtosList(1);

        var handlerMock = new Mock<IGetPollLinksHandler>();
        handlerMock
            .Setup(h => h.HandleAsync(0, limit))
            .ReturnsAsync(expectedData);

        var controller = new PollsController();

        // Act
        var result = await controller.GetPollLinksAsync(0, limit, handlerMock.Object);

        // Assert
        handlerMock.Verify(h => h.HandleAsync(0, limit), Times.Once);

        result
            .Should().ContainSingle()
            .Which.Should().BeEquivalentTo(expectedData[0]);
    }

    [TestCase(null, null)]
    [TestCase(-1, -1)]
    [TestCase(0, 0)]
    [TestCase(0, 10)]
    [TestCase(10, 10)]
    public async Task GetPollLinksAsync_ReturnsEmptyList_WhenHandlerReturnsNoData(int? offset, int? limit)
    {
        // Arrange
        var emptyData = new List<PollLinkDto>();
        var handlerMock = new Mock<IGetPollLinksHandler>();
        handlerMock
            .Setup(h => h.HandleAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(emptyData);

        var controller = new PollsController();

        // Act
        var result = await controller.GetPollLinksAsync(offset, limit, handlerMock.Object);

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public async Task GetPollByIdAsync_ReturnsOk_WhenPollExists()
    {
        // Arrange
        var expectedPollDto = TestDbHelper.CreatePollDto();
        var successResponse =
            new Response<PollDto>(
                Value: expectedPollDto,
                IsSuccess: true,
                Message: null,
                ErrorDetails: null,
                ErrorType: ErrorType.None);

        var handlerMock = new Mock<IGetPollByIdHandler>();
        handlerMock
            .Setup(h => h.HandleAsync(expectedPollDto.PollId))
            .ReturnsAsync(successResponse);

        var controller = new PollsController
        {
            ControllerContext = new ControllerContext()
        };

        // Act
        var actionResult = await controller.GetPollByIdAsync(expectedPollDto.PollId, handlerMock.Object);

        // Assert
        actionResult
            .Result.Should().BeOfType<OkObjectResult>()
            .Which
                .Value.Should().BeOfType<PollDto>()
                .Which.Should().BeEquivalentTo(expectedPollDto);
    }

    [Test]
    public async Task GetPollByIdAsync_ReturnsNotFound_WhenPollDoesNotExist()
    {
        // Arrange
        var pollId = Guid.NewGuid();
        var errorResponse =
            new Response<PollDto>(
                Value: null,
                IsSuccess: false,
                Message: "Опрос не найден!",
                ErrorDetails: $"Нет опроса с Id = {pollId}",
                ErrorType: ErrorType.EntityNotFound);

        var handlerMock = new Mock<IGetPollByIdHandler>();
        handlerMock
            .Setup(h => h.HandleAsync(pollId))
            .ReturnsAsync(errorResponse);

        var controller = new PollsController
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        // Act
        var actionResult = await controller.GetPollByIdAsync(pollId, handlerMock.Object);

        // Assert
        actionResult
            .Result.Should().BeOfType<NotFoundObjectResult>()
            .Which
                .Value.Should().BeOfType<ProblemDetails>()
                .Which.Should().BeEquivalentTo(
                new
                {
                    Status = 404,
                    Title = "Ошибка!",
                    Detail = "Опрос не найден!"
                });

        controller.HttpContext.Items
            .Should().ContainKey("ErrorDetails")
                .WhoseValue.Should().BeOfType<string>()
                    .Which.Should().Be($"Опрос не найден! Нет опроса с Id = {pollId}");
    }

    [Test]
    public async Task GetPollByIdAsync_ThrowsException_WhenIdIsHackGuid()
    {
        // Arrange
        var hackId = Guid.Parse("019c1aa8-9bf0-750d-9e6d-832de94b1c13");
        var handlerMock = new Mock<IGetPollByIdHandler>();
        var controller = new PollsController();

        // Act & Assert
        await controller
            .Awaiting(c => c.GetPollByIdAsync(hackId, handlerMock.Object))
            .Should().ThrowAsync<Exception>()
                .WithMessage("Тестовое исключение!");
    }
}
