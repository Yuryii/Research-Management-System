using MediatR.Pipeline;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RMS.Application.Application.Commands.CreateApplication;
using RMS.Application.Common.Behaviours;
using RMS.Application.Common.Interfaces;
using Shouldly;

namespace RMS.Application.UnitTests.Common.Behaviours;

public class LoggingBehaviourTests
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
    public async Task Process_LogsRequestName()
    {
        // Arrange
        var userId = "user-123";
        var userName = "TestUser";

        _userMock.Setup(u => u.Id).Returns(userId);
        _identityServiceMock.Setup(s => s.GetUserNameAsync(userId)).ReturnsAsync(userName);

        var sut = new LoggingBehaviour<CreateApplicationCommand>(
            _loggerMock.Object,
            _userMock.Object,
            _identityServiceMock.Object);

        var request = new CreateApplicationCommand { Title = "Test", Description = "Description" };

        // Act
        await sut.Process(request, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("RMS Request")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public async Task Process_LogsUserId_WhenUserIsAuthenticated()
    {
        // Arrange
        var userId = "user-123";
        var userName = "TestUser";

        _userMock.Setup(u => u.Id).Returns(userId);
        _identityServiceMock.Setup(s => s.GetUserNameAsync(userId)).ReturnsAsync(userName);

        var sut = new LoggingBehaviour<CreateApplicationCommand>(
            _loggerMock.Object,
            _userMock.Object,
            _identityServiceMock.Object);

        var request = new CreateApplicationCommand { Title = "Test", Description = "Description" };

        // Act
        await sut.Process(request, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(userId)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public async Task Process_LogsUserName_WhenUserIsAuthenticated()
    {
        // Arrange
        var userId = "user-123";
        var userName = "TestUser";

        _userMock.Setup(u => u.Id).Returns(userId);
        _identityServiceMock.Setup(s => s.GetUserNameAsync(userId)).ReturnsAsync(userName);

        var sut = new LoggingBehaviour<CreateApplicationCommand>(
            _loggerMock.Object,
            _userMock.Object,
            _identityServiceMock.Object);

        var request = new CreateApplicationCommand { Title = "Test", Description = "Description" };

        // Act
        await sut.Process(request, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(userName)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public async Task Process_LogsRequestObject()
    {
        // Arrange
        _userMock.Setup(u => u.Id).Returns((string?)null);

        var sut = new LoggingBehaviour<CreateApplicationCommand>(
            _loggerMock.Object,
            _userMock.Object,
            _identityServiceMock.Object);

        var request = new CreateApplicationCommand { Title = "Test", Description = "Description" };

        // Act
        await sut.Process(request, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("CreateApplicationCommand")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public async Task Process_DoesNotCallGetUserNameAsync_WhenUserIdIsNull()
    {
        // Arrange
        _userMock.Setup(u => u.Id).Returns((string?)null);

        var sut = new LoggingBehaviour<CreateApplicationCommand>(
            _loggerMock.Object,
            _userMock.Object,
            _identityServiceMock.Object);

        var request = new CreateApplicationCommand { Title = "Test", Description = "Description" };

        // Act
        await sut.Process(request, CancellationToken.None);

        // Assert
        _identityServiceMock.Verify(
            s => s.GetUserNameAsync(It.IsAny<string>()),
            Times.Never);
    }
}
