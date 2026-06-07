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
        // Arrange
        var command = new CreateStepCommand
        {
            Name = "New Step",
            ShortName = "NS",
            Order = 0
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Validate_ShouldFail_WhenNameIsEmpty()
    {
        // Arrange
        var command = new CreateStepCommand
        {
            Name = "",
            Order = 0
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Test]
    public void Validate_ShouldFail_WhenNameExceeds200Characters()
    {
        // Arrange
        var command = new CreateStepCommand
        {
            Name = new string('A', 201),
            Order = 0
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Test]
    public void Validate_ShouldFail_WhenShortNameExceeds200Characters()
    {
        // Arrange
        var command = new CreateStepCommand
        {
            Name = "Valid Name",
            ShortName = new string('B', 201),
            Order = 0
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ShortName);
    }

    [Test]
    public void Validate_ShouldFail_WhenOrderIsNegative()
    {
        // Arrange
        var command = new CreateStepCommand
        {
            Name = "Valid Name",
            Order = -1
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Order);
    }
}
