using System;
using NUnit.Framework;
using RMS.Application.Steps.Commands.UpdateStep;
using FluentValidation.TestHelper;

namespace RMS.Application.UnitTests.Steps.Commands;

public class UpdateStepCommandValidatorTests
{
    private readonly UpdateStepCommandValidator _validator = new();

    [Test]
    public void Validate_ShouldFail_WhenIdIsEmpty()
    {
        // Arrange
        var command = new UpdateStepCommand
        {
            Id = Guid.Empty,
            Name = "Updated Name"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Test]
    public void Validate_ShouldFail_WhenNameExceeds200Characters()
    {
        // Arrange
        var command = new UpdateStepCommand
        {
            Id = Guid.NewGuid(),
            Name = new string('A', 201)
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
        var command = new UpdateStepCommand
        {
            Id = Guid.NewGuid(),
            ShortName = new string('B', 201)
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
        var command = new UpdateStepCommand
        {
            Id = Guid.NewGuid(),
            Order = -1
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Order);
    }
}
