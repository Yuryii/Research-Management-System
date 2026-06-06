using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RMS.Application.Application.Commands.CreateApplication;
using RMS.Application.Common.Behaviours;
using Shouldly;

namespace RMS.Application.UnitTests.Common.Behaviours;

public class UnhandledExceptionBehaviourTests
{
    private Mock<ILogger<CreateApplicationCommand>> _loggerMock = null!;

    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<CreateApplicationCommand>>();
    }

    [Test]
    public async Task Handle_NoException_ReturnsResponse()
    {
        // Arrange
        var sut = new UnhandledExceptionBehaviour<CreateApplicationCommand, Guid>(_loggerMock.Object);

        var request = new CreateApplicationCommand { Title = "Test", Description = "Description" };
        var expectedResponse = Guid.NewGuid();
        RequestHandlerDelegate<Guid> next = (ct) => Task.FromResult(expectedResponse);

        // Act
        var result = await sut.Handle(request, next, CancellationToken.None);

        // Assert
        result.ShouldBe(expectedResponse);
    }

    [Test]
    public async Task Handle_ExceptionThrown_LogsErrorAndRethrows()
    {
        // Arrange
        var sut = new UnhandledExceptionBehaviour<CreateApplicationCommand, Guid>(_loggerMock.Object);

        var request = new CreateApplicationCommand { Title = "Test", Description = "Description" };
        var expectedException = new InvalidOperationException("Test exception");

        RequestHandlerDelegate<Guid> next = (ct) => throw expectedException;

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(() => sut.Handle(request, next, CancellationToken.None));

        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unhandled Exception")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public async Task Handle_ExceptionThrown_LogsRequestName()
    {
        // Arrange
        var sut = new UnhandledExceptionBehaviour<CreateApplicationCommand, Guid>(_loggerMock.Object);

        var request = new CreateApplicationCommand { Title = "Test", Description = "Description" };
        var expectedException = new ArgumentException("Invalid argument");

        RequestHandlerDelegate<Guid> next = (ct) => throw expectedException;

        // Act & Assert
        await Should.ThrowAsync<ArgumentException>(() => sut.Handle(request, next, CancellationToken.None));

        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("CreateApplicationCommand")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public async Task Handle_ExceptionThrown_RethrowsOriginalException()
    {
        // Arrange
        var sut = new UnhandledExceptionBehaviour<CreateApplicationCommand, Guid>(_loggerMock.Object);

        var request = new CreateApplicationCommand { Title = "Test", Description = "Description" };
        var expectedException = new DivideByZeroException("Cannot divide by zero");

        RequestHandlerDelegate<Guid> next = (ct) => throw expectedException;

        // Act & Assert
        var thrownException = await Should.ThrowAsync<DivideByZeroException>(
            () => sut.Handle(request, next, CancellationToken.None));

        thrownException.Message.ShouldBe(expectedException.Message);
    }
}
