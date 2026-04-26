using System;

using Ilnitsky.Polls.Contracts.Dtos;
using Ilnitsky.Polls.Extensions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Moq;

using RespExt = Ilnitsky.Polls.Extensions.ResponseExtensions;

namespace Ilnitsky.Polls.Tests.NUnit.Extensions;

public class ResponseExtensionsTests
{
    [TestCase(ErrorType.EntityNotFound, 404, typeof(NotFoundObjectResult))]
    [TestCase(ErrorType.IncorrectValue, 422, typeof(UnprocessableEntityObjectResult))]
    [TestCase(ErrorType.IncorrectFormat, 400, typeof(BadRequestObjectResult))]
    [TestCase(ErrorType.Error, 400, typeof(BadRequestObjectResult))]
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
        Assert.That(actionResult, Is.TypeOf(expectedType));

        var objectResult = actionResult as ObjectResult;

        Assert.That(objectResult?.Value, Is.TypeOf<ProblemDetails>());
        var problemDetails = (ProblemDetails)objectResult?.Value;

        Assert.Multiple(() =>
        {
            Assert.That(problemDetails.Status, Is.EqualTo(expectedStatus));
            Assert.That(problemDetails.Detail, Is.EqualTo("Test Error"));
            Assert.That(problemDetails.Title, Is.EqualTo("Ошибка!"));

            Assert.That(context.Items, Contains.Key("ErrorDetails"));
            Assert.That(context.Items["ErrorDetails"], Is.EqualTo("Test Error Test Details"));
        });
    }

    [Test]
    public void Response_GetActionResult_ReturnsOkWithData_WhenResponseIsSuccess()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var testData = new { Id = 1, Name = "Test" };
        var response = Response<object>.Success(testData);

        // Act
        var actionResult = response.GetActionResult(context);

        // Assert
        Assert.That(actionResult.Result, Is.TypeOf<OkObjectResult>());
        var okObjectResult = (OkObjectResult)actionResult.Result;

        Assert.Multiple(() =>
        {
            Assert.That(okObjectResult.StatusCode, Is.EqualTo(200));
            Assert.That(okObjectResult.Value, Is.EqualTo(testData));
        });
    }

    [Test]
    public void Response_GetActionResult_ReturnsError_WhenResponseIsFailure()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var response = Response<string>.EntityNotFound("Объект не найден", "Id=123");

        // Act
        var actionResult = response.GetActionResult(context);

        // Assert
        Assert.That(actionResult.Result, Is.TypeOf<NotFoundObjectResult>());
        var notFoundObjectResult = (NotFoundObjectResult)actionResult.Result;

        Assert.That(notFoundObjectResult.Value, Is.TypeOf<ProblemDetails>());
        var problemDetails = (ProblemDetails)notFoundObjectResult.Value;

        Assert.Multiple(() =>
        {
            Assert.That(notFoundObjectResult.StatusCode, Is.EqualTo(404));
            Assert.That(problemDetails.Status, Is.EqualTo(404));
            Assert.That(problemDetails.Detail, Is.EqualTo("Объект не найден"));
            Assert.That(problemDetails.Title, Is.EqualTo("Ошибка!"));

            Assert.That(context.Items, Contains.Key("ErrorDetails"));
            Assert.That(context.Items["ErrorDetails"], Is.EqualTo("Объект не найден Id=123"));
        });
    }

    [TestCase(true, typeof(CreatedResult), 201)]
    [TestCase(false, typeof(OkObjectResult), 200)]
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
        Assert.That(actionResult, Is.TypeOf(expectedType));
        var objectResult = actionResult as ObjectResult;
        Assert.That(objectResult?.StatusCode, Is.EqualTo(expectedStatusCode));
    }

    [Test]
    public void BaseResponse_GetActionResult_ReturnsError_WhenResponseIsFailure()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var response = BaseResponse.EntityNotFound("Объект не найден", "Id=123");

        // Act
        var actionResult = response.GetActionResult(context);

        // Assert
        Assert.That(actionResult, Is.InstanceOf<NotFoundObjectResult>());
        var notFoundObjectResult = (NotFoundObjectResult)actionResult;

        Assert.That(notFoundObjectResult.Value, Is.InstanceOf<ProblemDetails>());
        var problemDetails = (ProblemDetails)notFoundObjectResult.Value;

        Assert.Multiple(() =>
        {
            Assert.That(notFoundObjectResult.StatusCode, Is.EqualTo(404));
            Assert.That(problemDetails.Status, Is.EqualTo(404));
            Assert.That(problemDetails.Detail, Is.EqualTo("Объект не найден"));
            Assert.That(problemDetails.Title, Is.EqualTo("Ошибка!"));

            Assert.That(context.Items, Contains.Key("ErrorDetails"));
            Assert.That(context.Items["ErrorDetails"], Is.EqualTo("Объект не найден Id=123"));
        });
    }
}
