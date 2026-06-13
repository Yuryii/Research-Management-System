using NUnit.Framework;
using RMS.Application.Common.Exceptions;
using Shouldly;

namespace RMS.Application.UnitTests.Common.Exceptions;

public class ForbiddenAccessExceptionTests
{
    [Test]
    public void DefaultConstructor_ShouldCreateExceptionWithDefaultMessage()
    {
        // Arrange & Act
        var exception = new ForbiddenAccessException();

        // Assert
        exception.ShouldNotBeNull();
        exception.Message.ShouldNotBeNullOrEmpty();
    }

    [Test]
    public void ConstructorWithMessage_ShouldCreateExceptionWithProvidedMessage()
    {
        // Arrange
        var expectedMessage = "Access denied to this resource.";

        // Act
        var exception = new ForbiddenAccessException(expectedMessage);

        // Assert
        exception.ShouldNotBeNull();
        exception.Message.ShouldBe(expectedMessage);
    }

    [Test]
    public void Exception_ShouldBeAssignableFromSystemException()
    {
        // Arrange & Act
        var exception = new ForbiddenAccessException();

        // Assert
        exception.ShouldBeOfType<ForbiddenAccessException>();
        exception.ShouldBeAssignableTo<Exception>();
    }
}
