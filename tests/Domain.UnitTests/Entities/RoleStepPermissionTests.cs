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
        var permission = new RoleStepPermission
        {
            RoleId = "Administrator"
        };

        permission.RoleId.ShouldBe("Administrator");
    }

    [Test]
    public void ShouldHaveStepIdProperty()
    {
        var stepId = Guid.NewGuid();
        var permission = new RoleStepPermission
        {
            StepId = stepId
        };

        permission.StepId.ShouldBe(stepId);
    }

    [Test]
    public void ShouldHaveStepNavigation()
    {
        var step = new Step { Id = Guid.NewGuid(), Name = "Step 1" };
        var permission = new RoleStepPermission
        {
            StepId = step.Id,
            Step = step
        };

        permission.Step.ShouldBe(step);
    }

}
