using FluentValidation.TestHelper;
using NUnit.Framework;
using RMS.Application.Application.Commands.DeleteApplication;

namespace RMS.Application.UnitTests.Application.Commands;

public class DeleteApplicationCommandValidatorTests
{
    [Test]
    public async Task Validate_ShouldFail_WhenIdIsEmpty()
    {
        var validator = new DeleteApplicationCommandValidator();

        var command = new DeleteApplicationCommand(Guid.Empty);

        var result = await validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Application Id is required.");
    }

    [Test]
    public async Task Validate_ShouldPass_WhenIdIsValid()
    {
        var validator = new DeleteApplicationCommandValidator();

        var command = new DeleteApplicationCommand(Guid.NewGuid());

        var result = await validator.TestValidateAsync(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }
}
