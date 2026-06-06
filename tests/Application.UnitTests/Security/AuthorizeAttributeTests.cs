using NUnit.Framework;
using RMS.Application.Common.Security;
using Shouldly;

namespace RMS.Application.UnitTests.Security;

public class AuthorizeAttributeTests
{
    [Test]
    public void ShouldStoreRoles_WhenSet()
    {
        var attribute = new AuthorizeAttribute { Roles = "Teacher,Administrator" };

        attribute.Roles.ShouldBe("Teacher,Administrator");
    }

    [Test]
    public void ShouldStorePolicy_WhenSet()
    {
        var attribute = new AuthorizeAttribute { Policy = "CanManageApplications" };

        attribute.Policy.ShouldBe("CanManageApplications");
    }

    [Test]
    public void ShouldDefaultToEmptyRoles()
    {
        var attribute = new AuthorizeAttribute();

        attribute.Roles.ShouldBeEmpty();
    }

    [Test]
    public void ShouldDefaultToEmptyPolicy()
    {
        var attribute = new AuthorizeAttribute();

        attribute.Policy.ShouldBeEmpty();
    }

    [Test]
    public void ShouldAllowMultipleAttributes()
    {
        var attribute = new AuthorizeAttribute { Roles = "Teacher" };
        var second = new AuthorizeAttribute { Roles = "Administrator" };

        attribute.Roles.ShouldNotBe(second.Roles);
    }
}
