using System;
using NUnit.Framework;
using RMS.Application.Application.Commands.UpdateApplication;
using FluentValidation.TestHelper;

namespace RMS.Application.UnitTests.Application.Commands;

public class UpdateApplicationCommandValidatorTests
{
    private readonly UpdateApplicationCommandValidator _validator = new();

    [Test]
    public void Validate_ShouldPass_WhenCommandIsValid()
    {
        var command = new UpdateApplicationCommand
        {
            Id = Guid.NewGuid(),
            Title = "Updated Title",
            Description = "Updated Description"
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Validate_ShouldFail_WhenIdIsEmpty()
    {
        var command = new UpdateApplicationCommand
        {
            Id = Guid.Empty,
            Title = "Updated Title"
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Test]
    public void Validate_ShouldFail_WhenTitleExceeds100Characters()
    {
        var command = new UpdateApplicationCommand
        {
            Id = Guid.NewGuid(),
            Title = new string('A', 101)
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Test]
    public void Validate_ShouldFail_WhenDescriptionExceeds500Characters()
    {
        var command = new UpdateApplicationCommand
        {
            Id = Guid.NewGuid(),
            Description = new string('A', 501)
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Description);
    }
}
