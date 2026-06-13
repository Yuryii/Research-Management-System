using System;
using NUnit.Framework;
using RMS.Domain.Common;
using RMS.Domain.Entities.Models;
using Shouldly;

namespace RMS.Domain.UnitTests.Entities;

public class StepTests
{
    [Test]
    public void ShouldInitializeStepDetailsAsEmptyList()
    {
        // Arrange & Act
        var step = new Step { Id = Guid.NewGuid(), Name = "Step" };

        // Assert
        step.StepDetails.ShouldBeEmpty();
    }

    [Test]
    public void ShouldInitializeRoleStepPermissionsAsEmptyList()
    {
        // Arrange & Act
        var step = new Step { Id = Guid.NewGuid(), Name = "Step" };

        // Assert
        step.RoleStepPermissions.ShouldBeEmpty();
    }

    [Test]
    public void ShouldInitializeApplicationFilesAsEmptyList()
    {
        // Arrange & Act
        var step = new Step { Id = Guid.NewGuid(), Name = "Step" };

        // Assert
        step.ApplicationFiles.ShouldBeEmpty();
    }

    [Test]
    public void ShouldSetAndGetAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var nextStepId = Guid.NewGuid();

        // Act
        var step = new Step
        {
            Id = id,
            Name = "Approval Step",
            ShortName = "AP",
            Order = 5,
            NextStepId = nextStepId
        };

        // Assert
        step.Id.ShouldBe(id);
        step.Name.ShouldBe("Approval Step");
        step.ShortName.ShouldBe("AP");
        step.Order.ShouldBe(5);
        step.NextStepId.ShouldBe(nextStepId);
    }

    [Test]
    public void ShouldSetNextStep()
    {
        // Arrange
        var nextStep = new Step { Id = Guid.NewGuid(), Name = "Next Step" };

        // Act
        var step = new Step
        {
            Id = Guid.NewGuid(),
            Name = "Current Step",
            NextStep = nextStep
        };

        // Assert
        step.NextStep.ShouldBeSameAs(nextStep);
    }

    [Test]
    public void ShouldBeAuditable()
    {
        // Arrange & Act
        var step = new Step { Id = Guid.NewGuid(), Name = "Step" };

        // Assert
        step.ShouldBeAssignableTo<BaseAuditableEntity<Guid>>();
    }
}
