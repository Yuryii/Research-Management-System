using FluentValidation.TestHelper;
using NUnit.Framework;
using RMS.Application.Application.Commands.DeleteApplication;

namespace RMS.Application.UnitTests.Application.Commands;

public class DeleteApplicationCommandValidatorTests
{
    [Test]
    public async Task Validate_ShouldFail_WhenIdIsEmpty()
    {
        // Arrange
        var validator = new DeleteApplicationCommandValidator();

        var command = new DeleteApplicationCommand(Guid.Empty);

        // Act
        var result = await validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Application Id is required.");
    }

    [Test]
    public async Task Validate_ShouldPass_WhenIdIsValid()
    {
        // Arrange
        var validator = new DeleteApplicationCommandValidator();

        var command = new DeleteApplicationCommand(Guid.NewGuid());

        // Act
        var result = await validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }
}
