using System;
using NUnit.Framework;
using RMS.Application.Application.Commands.ReturnApplication;
using FluentValidation.TestHelper;

namespace RMS.Application.UnitTests.Application.Commands;

public class ReturnApplicationCommandValidatorTests
{
    private readonly ReturnApplicationCommandValidator _validator = new();

    [Test]
    public void Validate_ShouldFail_WhenApplicationIdIsEmpty()
    {
        var command = new ReturnApplicationCommand
        {
            ApplicationId = Guid.Empty,
            Title = "Return Title",
            Description = "Return Description"
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ApplicationId);
    }

    [Test]
    public void Validate_ShouldFail_WhenTitleIsEmpty()
    {
        var command = new ReturnApplicationCommand
        {
            ApplicationId = Guid.NewGuid(),
            Title = "",
            Description = "Return Description"
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Test]
    public void Validate_ShouldFail_WhenDescriptionIsEmpty()
    {
        var command = new ReturnApplicationCommand
        {
            ApplicationId = Guid.NewGuid(),
            Title = "Return Title",
            Description = ""
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Description);
    }
}
