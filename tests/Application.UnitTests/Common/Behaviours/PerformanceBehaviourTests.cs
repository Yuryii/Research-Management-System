using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RMS.Application.Application.Commands.CreateApplication;
using RMS.Application.Common.Behaviours;
using RMS.Application.Common.Interfaces;
using Shouldly;

namespace RMS.Application.UnitTests.Common.Behaviours;

public class PerformanceBehaviourTests
{
    private Mock<ILogger<CreateApplicationCommand>> _loggerMock = null!;
    private Mock<IUser> _userMock = null!;
    private Mock<IIdentityService> _identityServiceMock = null!;

    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<CreateApplicationCommand>>();
        _userMock = new Mock<IUser>();
        _identityServiceMock = new Mock<IIdentityService>();
    }

    [Test]
    public async Task Handle_FastRequest_DoesNotLogWarning()
    {
        // Arrange
        _userMock.Setup(u => u.Id).Returns((string?)null);

        var sut = new PerformanceBehaviour<CreateApplicationCommand, Guid>(
            _loggerMock.Object,
            _userMock.Object,
            _identityServiceMock.Object);

        var request = new CreateApplicationCommand { Title = "Test", Description = "Description" };
        var expectedResponse = Guid.NewGuid();
        RequestHandlerDelegate<Guid> next = (ct) => Task.FromResult(expectedResponse);

        // Act
        var result = await sut.Handle(request, next, CancellationToken.None);

        // Assert
        result.ShouldBe(expectedResponse);
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Test]
    public async Task Handle_SlowRequest_LogsWarning()
    {
        // Arrange
        _userMock.Setup(u => u.Id).Returns((string?)null);

        var sut = new PerformanceBehaviour<CreateApplicationCommand, Guid>(
            _loggerMock.Object,
            _userMock.Object,
            _identityServiceMock.Object);

        var request = new CreateApplicationCommand { Title = "Test", Description = "Description" };
        var expectedResponse = Guid.NewGuid();

        // Create a delegate that takes > 500ms
        RequestHandlerDelegate<Guid> next = async (ct) =>
        {
            await Task.Delay(600);
            return expectedResponse;
        };

        // Act
        var result = await sut.Handle(request, next, CancellationToken.None);

        // Assert
        result.ShouldBe(expectedResponse);
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Long Running Request")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public async Task Handle_SlowRequest_IncludesElapsedMillisecondsInLog()
    {
        // Arrange
        _userMock.Setup(u => u.Id).Returns((string?)null);

        var sut = new PerformanceBehaviour<CreateApplicationCommand, Guid>(
            _loggerMock.Object,
            _userMock.Object,
            _identityServiceMock.Object);

        var request = new CreateApplicationCommand { Title = "Test", Description = "Description" };
        var expectedResponse = Guid.NewGuid();

        RequestHandlerDelegate<Guid> next = async (ct) =>
        {
            await Task.Delay(600);
            return expectedResponse;
        };

        // Act
        await sut.Handle(request, next, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("milliseconds")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public async Task Handle_SlowRequest_LogsRequestName()
    {
        // Arrange
        _userMock.Setup(u => u.Id).Returns((string?)null);

        var sut = new PerformanceBehaviour<CreateApplicationCommand, Guid>(
            _loggerMock.Object,
            _userMock.Object,
            _identityServiceMock.Object);

        var request = new CreateApplicationCommand { Title = "Test", Description = "Description" };
        var expectedResponse = Guid.NewGuid();

        RequestHandlerDelegate<Guid> next = async (ct) =>
        {
            await Task.Delay(600);
            return expectedResponse;
        };

        // Act
        await sut.Handle(request, next, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("CreateApplicationCommand")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
