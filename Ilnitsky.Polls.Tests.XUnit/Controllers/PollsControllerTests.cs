using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Ilnitsky.Polls.BusinessLogic.Handlers.Polls;
using Ilnitsky.Polls.Contracts.Dtos;
using Ilnitsky.Polls.Contracts.Dtos.Polls;
using Ilnitsky.Polls.Controllers;
using Ilnitsky.Polls.Tests.Shared;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Moq;

namespace Ilnitsky.Polls.Tests.XUnit.Controllers;

public class PollsControllerTests
{
    [Theory]
    [InlineData(null, null)]
    [InlineData(null, -1)]
    [InlineData(-1, null)]
    [InlineData(-1, -1)]
    [InlineData(-10, -10)]
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

    [Theory]
    [InlineData(null, 1)]
    [InlineData(null, 10)]
    [InlineData(-1, 1)]
    [InlineData(-1, 10)]
    [InlineData(-10, 1)]
    [InlineData(-10, 10)]
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

    [Theory]
    [InlineData(0, null)]
    [InlineData(0, 0)]
    [InlineData(0, -1)]
    [InlineData(0, -10)]
    [InlineData(10, null)]
    [InlineData(10, 0)]
    [InlineData(10, -1)]
    [InlineData(10, -10)]
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

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
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

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(expectedData, result);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(-1, -1)]
    [InlineData(0, 0)]
    [InlineData(0, 10)]
    [InlineData(10, 10)]
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
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
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
        var okObjectResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedPollDto = Assert.IsType<PollDto>(okObjectResult.Value);

        Assert.Equivalent(expectedPollDto, returnedPollDto);
    }

    [Fact]
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
        var notFoundObjectResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        var problemDetails = Assert.IsType<ProblemDetails>(notFoundObjectResult.Value);

        Assert.Equal(404, problemDetails.Status);
        Assert.Equal("Ошибка!", problemDetails.Title);
        Assert.Equal("Опрос не найден!", problemDetails.Detail);

        Assert.True(controller.HttpContext.Items.ContainsKey("ErrorDetails"));
        var errorDetails = controller.HttpContext.Items["ErrorDetails"] as string;
        Assert.Equal($"Опрос не найден! Нет опроса с Id = {pollId}", errorDetails);
    }

    [Fact]
    public async Task GetPollByIdAsync_ThrowsException_WhenIdIsHackGuid()
    {
        // Arrange
        var hackId = Guid.Parse("019c1aa8-9bf0-750d-9e6d-832de94b1c13");
        var handlerMock = new Mock<IGetPollByIdHandler>();
        var controller = new PollsController();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            controller.GetPollByIdAsync(hackId, handlerMock.Object)
        );

        Assert.Equal("Тестовое исключение!", exception.Message);
    }
}
