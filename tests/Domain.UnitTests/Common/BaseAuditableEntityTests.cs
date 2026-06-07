using NUnit.Framework;
using RMS.Domain.Common;
using Shouldly;

namespace RMS.Domain.UnitTests.Common;

[TestFixture]
public class BaseAuditableEntityTests
{
    [Test]
    public void ShouldHaveCreatedProperty()
    {
        // Arrange
        var entity = new TestAuditableEntity();
        var now = DateTimeOffset.UtcNow;

        // Act
        entity.Created = now;

        // Assert
        entity.Created.ShouldBe(now);
    }

    [Test]
    public void ShouldHaveCreatedByProperty()
    {
        // Arrange
        var entity = new TestAuditableEntity();

        // Act
        entity.CreatedBy = "admin@test.com";

        // Assert
        entity.CreatedBy.ShouldBe("admin@test.com");
    }

    [Test]
    public void ShouldHaveLastModifiedProperty()
    {
        // Arrange
        var entity = new TestAuditableEntity();
        var now = DateTimeOffset.UtcNow;

        // Act
        entity.LastModified = now;

        // Assert
        entity.LastModified.ShouldBe(now);
    }

    [Test]
    public void ShouldHaveLastModifiedByProperty()
    {
        // Arrange
        var entity = new TestAuditableEntity();

        // Act
        entity.LastModifiedBy = "editor@test.com";

        // Assert
        entity.LastModifiedBy.ShouldBe("editor@test.com");
    }

    [Test]
    public void ShouldImplementIBaseAuditableEntity()
    {
        // Arrange
        var entity = new TestAuditableEntity();

        // Act & Assert
        entity.ShouldBeAssignableTo<IBaseAuditableEntity>();
    }

    private class TestAuditableEntity : BaseAuditableEntity<int>
    {
    }
}
