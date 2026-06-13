using System;
using NUnit.Framework;
using RMS.Domain.Common;
using RMS.Domain.Entities.Models;
using RMS.Domain.Enums;
using Shouldly;

namespace RMS.Domain.UnitTests.Entities;

public class ApplicationTests
{
    [Test]
    public void ShouldInitializeApplicationFilesAsEmptyList()
    {
        // Arrange & Act
        var app = new Application
        {
            Id = Guid.NewGuid(),
            Code = "APP-001",
            Title = "Test",
            Description = "Test"
        };

        // Assert
        app.ApplicationFiles.ShouldBeEmpty();
    }

    [Test]
    public void ShouldInitializeStatusAsDefault()
    {
        // Arrange & Act
        var app = new Application
        {
            Id = Guid.NewGuid(),
            Code = "APP-001",
            Title = "Test",
            Description = "Test"
        };

        // Assert
        app.Status.ShouldBe(default(ApplicationStatus));
    }

    [Test]
    public void ShouldSetAndGetAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var stepDetailId = Guid.NewGuid();

        // Act
        var app = new Application
        {
            Id = id,
            Code = "APP-001",
            Title = "Research Proposal",
            Description = "A research proposal about AI",
            Status = ApplicationStatus.Draft,
            StepDetailId = stepDetailId
        };

        // Assert
        app.Id.ShouldBe(id);
        app.Code.ShouldBe("APP-001");
        app.Title.ShouldBe("Research Proposal");
        app.Description.ShouldBe("A research proposal about AI");
        app.Status.ShouldBe(ApplicationStatus.Draft);
        app.StepDetailId.ShouldBe(stepDetailId);
    }

    [Test]
    public void ShouldSetParentApplicationId()
    {
        // Arrange
        var parentId = Guid.NewGuid();

        // Act
        var app = new Application
        {
            Id = Guid.NewGuid(),
            Code = "APP-001",
            Title = "Test",
            Description = "Test",
            ParentApplicationId = parentId
        };

        // Assert
        app.ParentApplicationId.ShouldBe(parentId);
    }

    [Test]
    public void ShouldSetParentApplicationIdToNull()
    {
        // Arrange & Act
        var app = new Application
        {
            Id = Guid.NewGuid(),
            Code = "APP-001",
            Title = "Test",
            Description = "Test",
            ParentApplicationId = null
        };

        // Assert
        app.ParentApplicationId.ShouldBeNull();
    }

    [Test]
    public void ShouldBeAuditable()
    {
        // Arrange & Act
        var app = new Application
        {
            Id = Guid.NewGuid(),
            Code = "APP-001",
            Title = "Test",
            Description = "Test"
        };

        // Assert
        app.ShouldBeAssignableTo<BaseAuditableEntity<Guid>>();
    }

    [Test]
    public void ShouldSupportGuidId()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var app = new Application
        {
            Id = id,
            Code = "APP-001",
            Title = "Test",
            Description = "Test"
        };

        // Assert
        app.Id.ShouldNotBe(default(Guid));
        app.Id.ShouldBe(id);
    }
}
