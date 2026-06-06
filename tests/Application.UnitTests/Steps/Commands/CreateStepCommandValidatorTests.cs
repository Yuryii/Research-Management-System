using System;
using NUnit.Framework;
using RMS.Application.Steps.Commands.CreateStep;
using FluentValidation.TestHelper;

namespace RMS.Application.UnitTests.Steps.Commands;

public class CreateStepCommandValidatorTests
{
    private readonly CreateStepCommandValidator _validator = new();

    [Test]
    public void Validate_ShouldPass_WhenCommandIsValid()
    {
        var command = new CreateStepCommand
        {
            Name = "New Step",
            ShortName = "NS",
            Order = 0
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Validate_ShouldFail_WhenNameIsEmpty()
    {
        var command = new CreateStepCommand
        {
            Name = "",
            Order = 0
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Test]
    public void Validate_ShouldFail_WhenNameExceeds200Characters()
    {
        var command = new CreateStepCommand
        {
            Name = new string('A', 201),
            Order = 0
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Test]
    public void Validate_ShouldFail_WhenShortNameExceeds200Characters()
    {
        var command = new CreateStepCommand
        {
            Name = "Valid Name",
            ShortName = new string('B', 201),
            Order = 0
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ShortName);
    }

    [Test]
    public void Validate_ShouldFail_WhenOrderIsNegative()
    {
        var command = new CreateStepCommand
        {
            Name = "Valid Name",
            Order = -1
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Order);
    }
}
