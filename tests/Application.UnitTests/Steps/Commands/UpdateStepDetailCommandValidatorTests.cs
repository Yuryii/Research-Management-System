using System;
using NUnit.Framework;
using RMS.Application.Steps.Commands.UpdateStepDetail;
using FluentValidation.TestHelper;

namespace RMS.Application.UnitTests.Steps.Commands;

public class UpdateStepDetailCommandValidatorTests
{
    private readonly UpdateStepDetailCommandValidator _validator = new();

    [Test]
    public void Validate_ShouldFail_WhenIdIsEmpty()
    {
        var command = new UpdateStepDetailCommand
        {
            Id = Guid.Empty,
            Name = "Updated Name"
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Test]
    public void Validate_ShouldFail_WhenNameExceeds200Characters()
    {
        var command = new UpdateStepDetailCommand
        {
            Id = Guid.NewGuid(),
            Name = new string('A', 201)
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Test]
    public void Validate_ShouldFail_WhenOrderIsNegative()
    {
        var command = new UpdateStepDetailCommand
        {
            Id = Guid.NewGuid(),
            Order = -1
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Order);
    }
}
