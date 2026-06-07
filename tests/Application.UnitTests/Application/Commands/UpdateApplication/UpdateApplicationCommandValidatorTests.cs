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
        // Arrange
        var command = new UpdateApplicationCommand
        {
            Id = Guid.NewGuid(),
            Title = "Updated Title",
            Description = "Updated Description"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Validate_ShouldFail_WhenIdIsEmpty()
    {
        // Arrange
        var command = new UpdateApplicationCommand
        {
            Id = Guid.Empty,
            Title = "Updated Title"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Test]
    public void Validate_ShouldFail_WhenTitleExceeds200Characters()
    {
        // Arrange
        var command = new UpdateApplicationCommand
        {
            Id = Guid.NewGuid(),
            Title = new string('A', 202)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }
    [Test]
    public void Validate_ShouldPass_WhenTitleIsAtMaxLength()
    {
        // Arrange
        var command = new UpdateApplicationCommand
        {
            Id = Guid.NewGuid(),
            Title = new string('A', 200)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    [Test]
    public void Validate_ShouldPass_WhenDescriptionIsAtMaxLength()
    {
        // Arrange
        var command = new UpdateApplicationCommand
        {
            Id = Guid.NewGuid(),
            Description = new string('A', 500)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }
    [Test]
    public void Validate_ShouldFail_WhenDescriptionExceeds500Characters()
    {
        // Arrange
        var command = new UpdateApplicationCommand
        {
            Id = Guid.NewGuid(),
            Description = new string('A', 501)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }
}
