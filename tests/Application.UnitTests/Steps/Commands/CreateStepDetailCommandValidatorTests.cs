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
        var command = new CreateStepDetailCommand
        {
            StepId = Guid.Empty,
            Name = "Step Detail Name",
            Order = 0
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.StepId);
    }

    [Test]
    public void Validate_ShouldFail_WhenNameIsEmpty()
    {
        var command = new CreateStepDetailCommand
        {
            StepId = Guid.NewGuid(),
            Name = "",
            Order = 0
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Test]
    public void Validate_ShouldFail_WhenOrderIsNegative()
    {
        var command = new CreateStepDetailCommand
        {
            StepId = Guid.NewGuid(),
            Name = "Valid Name",
            Order = -1
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Order);
    }

    [Test]
    public void Validate_ShouldFail_WhenNextStepDetailIdEqualsStepId()
    {
        var stepId = Guid.NewGuid();
        var command = new CreateStepDetailCommand
        {
            StepId = stepId,
            Name = "Valid Name",
            Order = 0,
            NextStepDetailId = stepId
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.NextStepDetailId);
    }

    [Test]
    public void Validate_ShouldFail_WhenIsReturnStepAndHasNextStepDetail()
    {
        var command = new CreateStepDetailCommand
        {
            StepId = Guid.NewGuid(),
            Name = "Return Step",
            Order = 0,
            IsReturnStep = true,
            NextStepDetailId = Guid.NewGuid()
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.IsReturnStep);
    }
}
