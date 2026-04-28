using System;
using System.Collections.Generic;

using FluentAssertions;

using Ilnitsky.Polls.Contracts.Dtos;
using Ilnitsky.Polls.Extensions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Moq;

using RespExt = Ilnitsky.Polls.Extensions.ResponseExtensions;

namespace Ilnitsky.Polls.Tests.XUnit.Fluent.Extensions;

public class ResponseExtensionsTests
{
    [Theory]
    [InlineData(ErrorType.EntityNotFound, 404, typeof(NotFoundObjectResult))]
    [InlineData(ErrorType.IncorrectValue, 422, typeof(UnprocessableEntityObjectResult))]
    [InlineData(ErrorType.IncorrectFormat, 400, typeof(BadRequestObjectResult))]
    [InlineData(ErrorType.Error, 400, typeof(BadRequestObjectResult))]
    public void GetError_ReturnsCorrectStatusCodeAndType(ErrorType errorType, int expectedStatus, Type expectedType)
    {
        // Arrange
        var context = new DefaultHttpContext();
        var responseMock = new Mock<IErrorInfo>();

        responseMock.Setup(r => r.ErrorType).Returns(errorType);
        responseMock.Setup(r => r.Message).Returns("Test Error");
        responseMock.Setup(r => r.ErrorDetails).Returns("Test Details");

        // Act
        var actionResult = RespExt.GetError(responseMock.Object, context);

        // Assert
        actionResult
            .Should().BeOfType(expectedType)
            .And
            .BeAssignableTo<ObjectResult>()
            .Which
                .Value.Should().BeOfType<ProblemDetails>()
                .Which.Should().BeEquivalentTo(
                new
                {
                    Status = expectedStatus,
                    Detail = "Test Error",
                    Title = "Ошибка!"
                });

        // v1
        context.Items
            .Should().Contain(new KeyValuePair<object, object?>("ErrorDetails", "Test Error Test Details"));

        // v2
        context.Items
            .Should().ContainKey("ErrorDetails")
                .WhoseValue.Should().Be("Test Error Test Details");
    }

    [Fact]
    public void Response_GetActionResult_ReturnsOkWithData_WhenResponseIsSuccess()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var testData = new { Id = 1, Name = "Test" };
        var response = Response<object>.Success(testData);

        // Act
        var actionResult = response.GetActionResult(context);

        // Assert
        actionResult
            .Result.Should().BeOfType<OkObjectResult>()
            .Which
                .Value.Should().BeEquivalentTo(testData);
    }

    [Fact]
    public void Response_GetActionResult_ReturnsError_WhenResponseIsFailure()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var response = Response<string>.EntityNotFound("Объект не найден", "Id=123");

        // Act
        var actionResult = response.GetActionResult(context);

        // Assert
        actionResult
            .Result.Should().BeOfType<NotFoundObjectResult>()
            .Which
                .Value.Should().BeOfType<ProblemDetails>()
                .Which.Should().BeEquivalentTo(
                new
                {
                    Status = 404,
                    Detail = "Объект не найден",
                    Title = "Ошибка!"
                });

        context
            .Items.Should().ContainKey("ErrorDetails")
                .WhoseValue.Should().Be("Объект не найден Id=123");
    }

    [Theory]
    [InlineData(true, typeof(CreatedResult), 201)]
    [InlineData(false, typeof(OkObjectResult), 200)]
    public void BaseResponse_GetActionResult_ReturnsCorrectStatus_WhenResponseIsSuccess(
        bool isCreated, Type expectedType, int expectedStatusCode)
    {
        // Arrange
        var context = new DefaultHttpContext();
        var response = new BaseResponse(
            IsSuccess: true,
            IsCreated: isCreated,
            Message: "Success",
            ErrorDetails: null,
            ErrorType: ErrorType.None);

        // Act
        var actionResult = response.GetActionResult(context);

        // Assert
        actionResult.Should().BeOfType(expectedType);
        actionResult.Should().BeAssignableTo<ObjectResult>()
                .Which.StatusCode.Should().Be(expectedStatusCode);
    }

    [Fact]
    public void BaseResponse_GetActionResult_ReturnsError_WhenResponseIsFailure()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var response = BaseResponse.EntityNotFound("Объект не найден", "Id=123");

        // Act
        var actionResult = response.GetActionResult(context);

        // Assert
        actionResult
            .Should().BeOfType<NotFoundObjectResult>()
            .Which
                .Value.Should().BeOfType<ProblemDetails>()
                .Which.Should().BeEquivalentTo(
                new
                {
                    Status = 404,
                    Detail = "Объект не найден",
                    Title = "Ошибка!"
                });

        context
            .Items.Should().ContainKey("ErrorDetails")
                .WhoseValue.Should().Be("Объект не найден Id=123");
    }
}
