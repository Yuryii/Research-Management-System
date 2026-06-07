using NUnit.Framework;
using RMS.Application.Common.Security;
using Shouldly;

namespace RMS.Application.UnitTests.Security;

public class AuthorizeAttributeTests
{
    [Test]
    public void ShouldStoreRoles_WhenSet()
    {
        // Arrange
        var attribute = new AuthorizeAttribute { Roles = "Teacher,Administrator" };

        // Act & Assert
        attribute.Roles.ShouldBe("Teacher,Administrator");
    }

    [Test]
    public void ShouldStorePolicy_WhenSet()
    {
        // Arrange
        var attribute = new AuthorizeAttribute { Policy = "CanManageApplications" };

        // Act & Assert
        attribute.Policy.ShouldBe("CanManageApplications");
    }

    [Test]
    public void ShouldDefaultToEmptyRoles()
    {
        // Arrange
        var attribute = new AuthorizeAttribute();

        // Act & Assert
        attribute.Roles.ShouldBeEmpty();
    }

    [Test]
    public void ShouldDefaultToEmptyPolicy()
    {
        // Arrange
        var attribute = new AuthorizeAttribute();

        // Act & Assert
        attribute.Policy.ShouldBeEmpty();
    }

    [Test]
    public void ShouldAllowMultipleAttributes()
    {
        // Arrange
        var attribute = new AuthorizeAttribute { Roles = "Teacher" };
        var second = new AuthorizeAttribute { Roles = "Administrator" };

        // Act & Assert
        attribute.Roles.ShouldNotBe(second.Roles);
    }
}
