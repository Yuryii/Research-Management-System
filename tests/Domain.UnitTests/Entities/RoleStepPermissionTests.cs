using NUnit.Framework;
using RMS.Domain.Entities;
using RMS.Domain.Entities.Models;
using Shouldly;

namespace RMS.Domain.UnitTests.Entities;

[TestFixture]
public class RoleStepPermissionTests
{
    [Test]
    public void ShouldHaveRoleIdProperty()
    {
        // Arrange
        var permission = new RoleStepPermission
        {
            RoleId = "Administrator"
        };

        // Act & Assert
        permission.RoleId.ShouldBe("Administrator");
    }

    [Test]
    public void ShouldHaveStepIdProperty()
    {
        // Arrange
        var stepId = Guid.NewGuid();
        var permission = new RoleStepPermission
        {
            StepId = stepId
        };

        // Act & Assert
        permission.StepId.ShouldBe(stepId);
    }

    [Test]
    public void ShouldHaveStepNavigation()
    {
        // Arrange
        var step = new Step { Id = Guid.NewGuid(), Name = "Step 1" };
        var permission = new RoleStepPermission
        {
            StepId = step.Id,
            Step = step
        };

        // Act & Assert
        permission.Step.ShouldBe(step);
    }

}
