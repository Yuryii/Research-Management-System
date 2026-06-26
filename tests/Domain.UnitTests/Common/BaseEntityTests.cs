using NUnit.Framework;
using RMS.Domain.Common;
using RMS.Domain.Events;
using Shouldly;

namespace RMS.Domain.UnitTests.Common;

[TestFixture]
public class BaseEntityTests
{
    public class TestDomainEvent : BaseEvent
    {
    }

    [Test]
    public void DomainEvents_ShouldBeEmpty_WhenCreated()
    {
        // Arrange
        var entity = new TestEntity();

        // Act & Assert
        entity.DomainEvents.ShouldBeEmpty();
    }

    [Test]
    public void ShouldAddDomainEvent_WhenAddDomainEventCalled()
    {
        // Arrange
        var entity = new TestEntity();
        var domainEvent = new TestDomainEvent();

        // Act
        entity.AddDomainEvent(domainEvent);

        // Assert
        entity.DomainEvents.ShouldContain(domainEvent);
    }

    [Test]
    public void ShouldRemoveDomainEvent_WhenRemoveDomainEventCalled()
    {
        // Arrange
        var entity = new TestEntity();
        var domainEvent = new TestDomainEvent();
        entity.AddDomainEvent(domainEvent);

        // Act
        entity.RemoveDomainEvent(domainEvent);

        // Assert
        entity.DomainEvents.ShouldBeEmpty();
    }

    [Test]
    public void ShouldClearDomainEvents_WhenClearDomainEventsCalled()
    {
        // Arrange
        var entity = new TestEntity();
        entity.AddDomainEvent(new TestDomainEvent());
        entity.AddDomainEvent(new TestDomainEvent());

        // Act
        entity.ClearDomainEvents();

        // Assert
        entity.DomainEvents.ShouldBeEmpty();
    }

    [Test]
    public void DomainEvents_ShouldBeReadOnly()
    {
        // Arrange
        var entity = new TestEntity();

        // Act
        var domainEvents = entity.DomainEvents;

        // Assert
        domainEvents.ShouldBeAssignableTo<IReadOnlyCollection<BaseEvent>>();
    }

    [Test]
    public void ShouldTrackMultipleDomainEvents()
    {
        // Arrange
        var entity = new TestEntity();
        entity.AddDomainEvent(new TestDomainEvent());
        entity.AddDomainEvent(new TestDomainEvent());

        // Act & Assert
        entity.DomainEvents.Count.ShouldBe(2);
    }

    [Test]
    public void Id_ShouldBeNullable()
    {
        // Arrange
        var entity = new TestEntity();

        // Act & Assert
        entity.Id.ShouldBe(default(int));
    }

    [Test]
    public void ShouldSupportGuidGeneric()
    {
        // Arrange
        var entity = new GuidTestEntity();

        // Act & Assert
        entity.Id.ShouldBe(default(Guid));
        entity.Id = Guid.NewGuid();
        entity.Id.ShouldNotBe(default(Guid));
    }

    [Test]
    public void ShouldSupportIntGeneric()
    {
        // Arrange
        var entity = new IntTestEntity();

        // Act & Assert
        entity.Id.ShouldBe(default(int));
        entity.Id = 42;
        entity.Id.ShouldBe(42);
    }

    [Test]
    public void SameReference_ShouldBeEqual()
    {
        // Arrange
        var entity = new TestEntity();
        entity.Id = 1;

        // Act
        var sameEntity = entity;

        // Assert
        sameEntity.ShouldBeSameAs(entity);
    }

    private class TestEntity : BaseEntity<int>
    {
    }

    private class GuidTestEntity : BaseEntity<Guid>
    {
    }

    private class IntTestEntity : BaseEntity<int>
    {
    }
}
