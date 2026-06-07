using System;
using NUnit.Framework;
using RMS.Application.Application.Commands.DeleteApplicationFiles;
using FluentValidation.TestHelper;

namespace RMS.Application.UnitTests.Application.Commands;

public class DeleteApplicationFilesCommandValidatorTests
{
    private readonly DeleteApplicationFilesCommandValidator _validator = new();

    [Test]
    public void Validate_ShouldFail_WhenApplicationIdIsEmpty()
    {
        var command = new DeleteApplicationFilesCommand
        {
            ApplicationId = Guid.Empty,
            FileId = Guid.NewGuid()
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ApplicationId);
    }

    [Test]
    public void Validate_ShouldFail_WhenFileIdIsEmpty()
    {
        var command = new DeleteApplicationFilesCommand
        {
            ApplicationId = Guid.NewGuid(),
            FileId = Guid.Empty
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.FileId);
    }
}
