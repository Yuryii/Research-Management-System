using NUnit.Framework;
using RMS.Application.Common.Models;
using Shouldly;

namespace RMS.Application.UnitTests.Common.Models;

public class ResultTests
{
    [Test]
    public void Success_ShouldReturnSuccessResult()
    {
        // Arrange & Act
        var result = Result.Success();

        // Assert
        result.Succeeded.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Test]
    public void Failure_ShouldReturnFailureResult()
    {
        // Arrange
        var errors = new[] { "Error 1", "Error 2" };

        // Act
        var result = Result.Failure(errors);

        // Assert
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldBeEquivalentTo(errors);
    }

    [Test]
    public void Failure_WithSingleError_ShouldWork()
    {
        // Arrange & Act
        var result = Result.Failure(new[] { "Something went wrong" });

        // Assert
        result.Succeeded.ShouldBeFalse();
        result.Errors.Length.ShouldBe(1);
        result.Errors[0].ShouldBe("Something went wrong");
    }

    [Test]
    public void Success_ShouldHaveEmptyErrors()
    {
        // Arrange & Act
        var result = Result.Success();

        // Assert
        result.Errors.ShouldNotBeNull();
        result.Errors.Length.ShouldBe(0);
    }

    [Test]
    public void Failure_WithEmptyEnumerable_ShouldWork()
    {
        // Arrange & Act
        var result = Result.Failure(Array.Empty<string>());

        // Assert
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldBeEmpty();
    }

    [Test]
    public void Failure_ShouldPreserveErrorOrder()
    {
        // Arrange
        var errors = new[] { "First", "Second", "Third" };

        // Act
        var result = Result.Failure(errors);

        // Assert
        result.Errors[0].ShouldBe("First");
        result.Errors[1].ShouldBe("Second");
        result.Errors[2].ShouldBe("Third");
    }
}
