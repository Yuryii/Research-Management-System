using System;
using NUnit.Framework;
using RMS.Application.Application.Commands.ForwardNextToStep;
using FluentValidation.TestHelper;

namespace RMS.Application.UnitTests.Application.Commands;

public class ForwardNextToStepCommandValidatorTests
{
    private readonly ForwardNextToStepCommandValidator _validator = new();

    [Test]
    public void Validate_ShouldFail_WhenApplicationIdIsEmpty()
    {
        // Arrange
        var command = new ForwardNextToStepCommand
        {
            ApplicationId = Guid.Empty
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ApplicationId)
            .WithErrorMessage("Application ID is required.");
    }
}
