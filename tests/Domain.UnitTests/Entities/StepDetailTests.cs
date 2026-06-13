using System;
using NUnit.Framework;
using RMS.Domain.Common;
using RMS.Domain.Entities.Models;
using Shouldly;

namespace RMS.Domain.UnitTests.Entities;

public class StepDetailTests
{
    [Test]
    public void ShouldInitializeApplicationsAsEmptyList()
    {
        // Arrange & Act
        var detail = new StepDetail
        {
            Id = Guid.NewGuid(),
            StepId = Guid.NewGuid(),
            Name = "Detail"
        };

        // Assert
        detail.Applications.ShouldBeEmpty();
    }

    [Test]
    public void ShouldSetAndGetAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var stepId = Guid.NewGuid();
        var nextStepDetailId = Guid.NewGuid();

        // Act
        var detail = new StepDetail
        {
            Id = id,
            StepId = stepId,
            Name = "Review Detail",
            Order = 3,
            NextStepDetailId = nextStepDetailId,
            IsReturnStep = true,
            IsCaculateScoreStep = false
        };

        // Assert
        detail.Id.ShouldBe(id);
        detail.StepId.ShouldBe(stepId);
        detail.Name.ShouldBe("Review Detail");
        detail.Order.ShouldBe(3);
        detail.NextStepDetailId.ShouldBe(nextStepDetailId);
        detail.IsReturnStep.ShouldBeTrue();
        detail.IsCaculateScoreStep.ShouldBeFalse();
    }

    [Test]
    public void ShouldSetNextStepDetail()
    {
        // Arrange
        var nextDetail = new StepDetail
        {
            Id = Guid.NewGuid(),
            StepId = Guid.NewGuid(),
            Name = "Next Detail"
        };

        // Act
        var detail = new StepDetail
        {
            Id = Guid.NewGuid(),
            StepId = Guid.NewGuid(),
            Name = "Current Detail",
            NextStepDetail = nextDetail
        };

        // Assert
        detail.NextStepDetail.ShouldBeSameAs(nextDetail);
    }

    [Test]
    public void ShouldSetIsReturnStep()
    {
        // Arrange & Act
        var detail = new StepDetail
        {
            Id = Guid.NewGuid(),
            StepId = Guid.NewGuid(),
            Name = "Detail",
            IsReturnStep = true
        };

        // Assert
        detail.IsReturnStep.ShouldBeTrue();
    }

    [Test]
    public void ShouldSetIsCaculateScoreStep()
    {
        // Arrange & Act
        var detail = new StepDetail
        {
            Id = Guid.NewGuid(),
            StepId = Guid.NewGuid(),
            Name = "Detail",
            IsCaculateScoreStep = true
        };

        // Assert
        detail.IsCaculateScoreStep.ShouldBeTrue();
    }

    [Test]
    public void ShouldBeAuditable()
    {
        // Arrange & Act
        var detail = new StepDetail
        {
            Id = Guid.NewGuid(),
            StepId = Guid.NewGuid(),
            Name = "Detail"
        };

        // Assert
        detail.ShouldBeAssignableTo<BaseAuditableEntity<Guid>>();
    }
}
