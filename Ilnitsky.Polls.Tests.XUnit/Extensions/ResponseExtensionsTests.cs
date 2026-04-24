using System;

using Ilnitsky.Polls.Contracts.Dtos;
using Ilnitsky.Polls.Extensions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Moq;

using RespExt = Ilnitsky.Polls.Extensions.ResponseExtensions;

namespace Ilnitsky.Polls.Tests.XUnit.Extensions;

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
        Assert.IsType(expectedType, actionResult);

        var objectResult = actionResult as ObjectResult;
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult?.Value);

        Assert.Equal(expectedStatus, problemDetails.Status);
        Assert.Equal("Test Error", problemDetails.Detail);
        Assert.Equal("Ошибка!", problemDetails.Title);

        Assert.True(context.Items.ContainsKey("ErrorDetails"));
        Assert.Equal("Test Error Test Details", context.Items["ErrorDetails"]);
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
        var okObjectResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(200, okObjectResult.StatusCode);
        Assert.Equal(testData, okObjectResult.Value);
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
        var notFoundObjectResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        var problemDetails = Assert.IsType<ProblemDetails>(notFoundObjectResult.Value);

        Assert.Equal(404, notFoundObjectResult.StatusCode);
        Assert.Equal(404, problemDetails.Status);
        Assert.Equal("Объект не найден", problemDetails.Detail);
        Assert.Equal("Ошибка!", problemDetails.Title);

        Assert.True(context.Items.ContainsKey("ErrorDetails"));
        Assert.Equal("Объект не найден Id=123", context.Items["ErrorDetails"]);
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
        Assert.IsType(expectedType, actionResult);
        var objectResult = actionResult as ObjectResult;
        Assert.Equal(expectedStatusCode, objectResult?.StatusCode);
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
        var notFoundObjectResult = Assert.IsType<NotFoundObjectResult>(actionResult);
        var problemDetails = Assert.IsType<ProblemDetails>(notFoundObjectResult.Value);

        Assert.Equal(404, notFoundObjectResult.StatusCode);
        Assert.Equal(404, problemDetails.Status);
        Assert.Equal("Объект не найден", problemDetails.Detail);
        Assert.Equal("Ошибка!", problemDetails.Title);

        Assert.True(context.Items.ContainsKey("ErrorDetails"));
        Assert.Equal("Объект не найден Id=123", context.Items["ErrorDetails"]);
    }
}
