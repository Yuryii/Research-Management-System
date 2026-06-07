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
        // Arrange
        var command = new CreateApplicationCommand
        {
            Title = new string('A', 200),
            Description = new string('B', 1000)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Validate_ShouldFail_WhenTitleIsEmpty()
    {
        // Arrange
        var command = new CreateApplicationCommand
        {
            Title = "",
            Description = "Valid Description"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Test]
    public void Validate_ShouldFail_WhenTitleExceeds200Characters()
    {
        // Arrange
        var command = new CreateApplicationCommand
        {
            Title = new string('A', 201),
            Description = "Valid Description"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Test]
    public void Validate_ShouldFail_WhenDescriptionIsEmpty()
    {
        // Arrange
        var command = new CreateApplicationCommand
        {
            Title = "Valid Title",
            Description = ""
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Test]
    public void Validate_ShouldFail_WhenDescriptionExceeds1000Characters()
    {
        // Arrange
        var command = new CreateApplicationCommand
        {
            Title = "Valid Title",
            Description = new string('A', 1001)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }
}
