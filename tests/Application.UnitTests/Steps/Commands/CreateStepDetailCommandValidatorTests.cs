using System;
using NUnit.Framework;
using RMS.Application.Steps.Commands.CreateStepDetail;
using FluentValidation.TestHelper;

namespace RMS.Application.UnitTests.Steps.Commands;

public class CreateStepDetailCommandValidatorTests
{
    private readonly CreateStepDetailCommandValidator _validator = new();

    [Test]
    public void Validate_ShouldFail_WhenStepIdIsEmpty()
    {
        // Arrange
        var command = new CreateStepDetailCommand
        {
            StepId = Guid.Empty,
            Name = "Step Detail Name",
            Order = 0
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.StepId);
    }

    [Test]
    public void Validate_ShouldFail_WhenNameIsEmpty()
    {
        // Arrange
        var command = new CreateStepDetailCommand
        {
            StepId = Guid.NewGuid(),
            Name = "",
            Order = 0
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Test]
    public void Validate_ShouldFail_WhenOrderIsNegative()
    {
        // Arrange
        var command = new CreateStepDetailCommand
        {
            StepId = Guid.NewGuid(),
            Name = "Valid Name",
            Order = -1
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Order);
    }

    [Test]
    public void Validate_ShouldFail_WhenNextStepDetailIdEqualsStepId()
    {
        // Arrange
        var stepId = Guid.NewGuid();
        var command = new CreateStepDetailCommand
        {
            StepId = stepId,
            Name = "Valid Name",
            Order = 0,
            NextStepDetailId = stepId
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NextStepDetailId);
    }

    [Test]
    public void Validate_ShouldFail_WhenIsReturnStepAndHasNextStepDetail()
    {
        // Arrange
        var command = new CreateStepDetailCommand
        {
            StepId = Guid.NewGuid(),
            Name = "Return Step",
            Order = 0,
            IsReturnStep = true,
            NextStepDetailId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.IsReturnStep);
    }
}
