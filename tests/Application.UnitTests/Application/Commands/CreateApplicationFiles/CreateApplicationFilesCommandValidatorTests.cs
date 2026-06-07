using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using RMS.Application.Application.Commands.CreateApplicationFiles;
using FluentValidation.TestHelper;

namespace RMS.Application.UnitTests.Application.Commands;

public class CreateApplicationFilesCommandValidatorTests
{
    private readonly CreateApplicationFilesCommandValidator _validator = new();

    [Test]
    public void Validate_ShouldPass_WhenCommandIsValid()
    {
        // Arrange
        var command = new CreateApplicationFilesCommand
        {
            ApplicationId = Guid.NewGuid(),
            Files = CreateValidFiles()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Validate_ShouldFail_WhenApplicationIdIsEmpty()
    {
        // Arrange
        var command = new CreateApplicationFilesCommand
        {
            ApplicationId = Guid.Empty,
            Files = CreateValidFiles()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ApplicationId);
    }

    [Test]
    public void Validate_ShouldFail_WhenFilesIsEmpty()
    {
        // Arrange
        var command = new CreateApplicationFilesCommand
        {
            ApplicationId = Guid.NewGuid(),
            Files = new FormFileCollection()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Files);
    }

    [Test]
    public void Validate_ShouldFail_WhenFilesIsNull()
    {
        // Arrange
        var command = new CreateApplicationFilesCommand
        {
            ApplicationId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Files);
    }

    private static FormFileCollection CreateValidFiles()
    {
        var stream = new MemoryStream(
            System.Text.Encoding.UTF8.GetBytes("Test"));

        var files = new FormFileCollection();
        files.Add(new FormFile(
            stream,
            0,
            stream.Length,
            "Files",
            "test.txt"));

        return files;
    }
}
