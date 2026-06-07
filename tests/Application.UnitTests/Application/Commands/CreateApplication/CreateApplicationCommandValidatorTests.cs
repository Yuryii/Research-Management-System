using System;
using NUnit.Framework;
using RMS.Application.Application.Commands.CreateApplication;
using FluentValidation.TestHelper;

namespace RMS.Application.UnitTests.Application.Commands;

public class CreateApplicationCommandValidatorTests
{
    private readonly CreateApplicationCommandValidator _validator = new();

    [Test]
    public void Validate_ShouldPass_WhenTitleAndDescriptionAreAtMaxLength()
    {
        var command = new CreateApplicationCommand
        {
            Title = new string('A', 200),
            Description = new string('B', 1000)
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Validate_ShouldFail_WhenTitleIsEmpty()
    {
        var command = new CreateApplicationCommand
        {
            Title = "",
            Description = "Valid Description"
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Test]
    public void Validate_ShouldFail_WhenTitleExceeds200Characters()
    {
        var command = new CreateApplicationCommand
        {
            Title = new string('A', 201),
            Description = "Valid Description"
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Test]
    public void Validate_ShouldFail_WhenDescriptionIsEmpty()
    {
        var command = new CreateApplicationCommand
        {
            Title = "Valid Title",
            Description = ""
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Test]
    public void Validate_ShouldFail_WhenDescriptionExceeds1000Characters()
    {
        var command = new CreateApplicationCommand
        {
            Title = "Valid Title",
            Description = new string('A', 1001)
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Description);
    }
}
