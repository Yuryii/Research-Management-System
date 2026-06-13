using System;
using NUnit.Framework;
using RMS.Domain.Entities.Models;
using Shouldly;
using DomainFile = RMS.Domain.Entities.Models.File;

namespace RMS.Domain.UnitTests.Entities;

public class ApplicationFileTests
{
    [Test]
    public void ShouldSetAndGetApplicationId()
    {
        // Arrange
        var applicationId = Guid.NewGuid();

        // Act
        var af = new ApplicationFile { ApplicationId = applicationId };

        // Assert
        af.ApplicationId.ShouldBe(applicationId);
    }

    [Test]
    public void ShouldSetAndGetFileId()
    {
        // Arrange
        var fileId = Guid.NewGuid();

        // Act
        var af = new ApplicationFile { FileId = fileId };

        // Assert
        af.FileId.ShouldBe(fileId);
    }

    [Test]
    public void ShouldSetAndGetStepId()
    {
        // Arrange
        var stepId = Guid.NewGuid();

        // Act
        var af = new ApplicationFile { StepId = stepId };

        // Assert
        af.StepId.ShouldBe(stepId);
    }

    [Test]
    public void ShouldSetNavigations()
    {
        // Arrange
        var app = new Application
        {
            Id = Guid.NewGuid(),
            Code = "APP-001",
            Title = "Test",
            Description = "Test"
        };

        var file = new DomainFile
        {
            Id = Guid.NewGuid(),
            Name = "doc.pdf",
            Path = "/files/doc.pdf",
            ContentType = "application/pdf",
            Length = 1024
        };

        var step = new Step
        {
            Id = Guid.NewGuid(),
            Name = "Step 1"
        };

        // Act
        var af = new ApplicationFile
        {
            ApplicationId = app.Id,
            FileId = file.Id,
            StepId = step.Id,
            Application = app,
            File = file,
            Step = step
        };

        // Assert
        af.Application.ShouldBeSameAs(app);
        af.File.ShouldBeSameAs(file);
        af.Step.ShouldBeSameAs(step);
    }
}
