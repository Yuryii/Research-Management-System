using NUnit.Framework;
using RMS.Application.Common.Models;
using Shouldly;

namespace RMS.Application.UnitTests.Common.Models;

public class ResultTests
{
    [Test]
    public void Success_ShouldReturnSuccessResult()
    {
        var result = Result.Success();

        result.Succeeded.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Test]
    public void Failure_ShouldReturnFailureResult()
    {
        var errors = new[] { "Error 1", "Error 2" };
        var result = Result.Failure(errors);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldBeEquivalentTo(errors);
    }

    [Test]
    public void Failure_WithSingleError_ShouldWork()
    {
        var result = Result.Failure(new[] { "Something went wrong" });

        result.Succeeded.ShouldBeFalse();
        result.Errors.Length.ShouldBe(1);
        result.Errors[0].ShouldBe("Something went wrong");
    }

    [Test]
    public void Success_ShouldHaveEmptyErrors()
    {
        var result = Result.Success();

        result.Errors.ShouldNotBeNull();
        result.Errors.Length.ShouldBe(0);
    }

    [Test]
    public void Failure_WithEmptyEnumerable_ShouldWork()
    {
        var result = Result.Failure(Array.Empty<string>());

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldBeEmpty();
    }

    [Test]
    public void Failure_ShouldPreserveErrorOrder()
    {
        var errors = new[] { "First", "Second", "Third" };
        var result = Result.Failure(errors);

        result.Errors[0].ShouldBe("First");
        result.Errors[1].ShouldBe("Second");
        result.Errors[2].ShouldBe("Third");
    }
}
