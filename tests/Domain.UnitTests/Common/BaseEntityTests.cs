using NUnit.Framework;
using RMS.Domain.Common;
using RMS.Domain.Entities;
using RMS.Domain.Events;
using Shouldly;

namespace RMS.Domain.UnitTests.Common;

[TestFixture]
public class BaseEntityTests
{
    [Test]
    public void DomainEvents_ShouldBeEmpty_WhenCreated()
    {
        var entity = new TestEntity();

        entity.DomainEvents.ShouldBeEmpty();
    }

    [Test]
    public void ShouldAddDomainEvent_WhenAddDomainEventCalled()
    {
        var entity = new TestEntity();
        var domainEvent = new TodoItemCompletedEvent(new TodoItem { Id = 1, ListId = 1 });

        entity.AddDomainEvent(domainEvent);

        entity.DomainEvents.ShouldContain(domainEvent);
    }

    [Test]
    public void ShouldRemoveDomainEvent_WhenRemoveDomainEventCalled()
    {
        var entity = new TestEntity();
        var domainEvent = new TodoItemCompletedEvent(new TodoItem { Id = 1, ListId = 1 });
        entity.AddDomainEvent(domainEvent);

        entity.RemoveDomainEvent(domainEvent);

        entity.DomainEvents.ShouldBeEmpty();
    }

    [Test]
    public void ShouldClearDomainEvents_WhenClearDomainEventsCalled()
    {
        var entity = new TestEntity();
        entity.AddDomainEvent(new TodoItemCompletedEvent(new TodoItem { Id = 1, ListId = 1 }));
        entity.AddDomainEvent(new TodoItemCompletedEvent(new TodoItem { Id = 2, ListId = 1 }));

        entity.ClearDomainEvents();

        entity.DomainEvents.ShouldBeEmpty();
    }

    [Test]
    public void DomainEvents_ShouldBeReadOnly()
    {
        var entity = new TestEntity();

        var domainEvents = entity.DomainEvents;

        domainEvents.ShouldBeAssignableTo<IReadOnlyCollection<BaseEvent>>();
    }

    [Test]
    public void ShouldTrackMultipleDomainEvents()
    {
        var entity = new TestEntity();
        entity.AddDomainEvent(new TodoItemCompletedEvent(new TodoItem { Id = 1, ListId = 1 }));
        entity.AddDomainEvent(new TodoItemCompletedEvent(new TodoItem { Id = 2, ListId = 1 }));

        entity.DomainEvents.Count.ShouldBe(2);
    }

    [Test]
    public void Id_ShouldBeNullable()
    {
        var entity = new TestEntity();

        entity.Id.ShouldBe(default(int));
    }

    [Test]
    public void ShouldSupportGuidGeneric()
    {
        var entity = new GuidTestEntity();

        entity.Id.ShouldBe(default(Guid));
        entity.Id = Guid.NewGuid();
        entity.Id.ShouldNotBe(default(Guid));
    }

    [Test]
    public void ShouldSupportIntGeneric()
    {
        var entity = new IntTestEntity();

        entity.Id.ShouldBe(default(int));
        entity.Id = 42;
        entity.Id.ShouldBe(42);
    }

    [Test]
    public void SameReference_ShouldBeEqual()
    {
        var entity = new TestEntity();
        entity.Id = 1;

        var sameEntity = entity;

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
