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
        var entity = new TestAuditableEntity();
        var now = DateTimeOffset.UtcNow;
        entity.Created = now;

        entity.Created.ShouldBe(now);
    }

    [Test]
    public void ShouldHaveCreatedByProperty()
    {
        var entity = new TestAuditableEntity();
        entity.CreatedBy = "admin@test.com";

        entity.CreatedBy.ShouldBe("admin@test.com");
    }

    [Test]
    public void ShouldHaveLastModifiedProperty()
    {
        var entity = new TestAuditableEntity();
        var now = DateTimeOffset.UtcNow;
        entity.LastModified = now;

        entity.LastModified.ShouldBe(now);
    }

    [Test]
    public void ShouldHaveLastModifiedByProperty()
    {
        var entity = new TestAuditableEntity();
        entity.LastModifiedBy = "editor@test.com";

        entity.LastModifiedBy.ShouldBe("editor@test.com");
    }

    [Test]
    public void ShouldImplementIBaseAuditableEntity()
    {
        var entity = new TestAuditableEntity();

        entity.ShouldBeAssignableTo<IBaseAuditableEntity>();
    }

    private class TestAuditableEntity : BaseAuditableEntity<int>
    {
    }
}
