using System;
using NUnit.Framework;
using RMS.Application.Application.Commands.UpdateApplicationStepDetail;
using FluentValidation.TestHelper;

namespace RMS.Application.UnitTests.Application.Commands;

public class UpdateApplicationStepDetailCommandValidatorTests
{
    private readonly UpdateApplicationStepDetailCommandValidator _validator = new();

    [Test]
    public void Validate_ShouldFail_WhenApplicationIdIsEmpty()
    {
        // Arrange
        var command = new UpdateApplicationStepDetailCommand
        {
            ApplicationId = Guid.Empty,
            StepDetailId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ApplicationId);
    }

    [Test]
    public void Validate_ShouldFail_WhenStepDetailIdIsEmpty()
    {
        // Arrange
        var command = new UpdateApplicationStepDetailCommand
        {
            ApplicationId = Guid.NewGuid(),
            StepDetailId = Guid.Empty
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.StepDetailId);
    }
}
