using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RMS.Domain.Common;

namespace RMS.Infrastructure.Data.Interceptors;

public class DispatchDomainEventsInterceptor : SaveChangesInterceptor
{
    private readonly IMediator _mediator;

    public DispatchDomainEventsInterceptor(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        DispatchDomainEvents(eventData.Context).GetAwaiter().GetResult();

        return base.SavingChanges(eventData, result);

    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        await DispatchDomainEvents(eventData.Context);

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public async Task DispatchDomainEvents(DbContext? context)
    {
        if (context is null) return;

        var entities = context.ChangeTracker
            .Entries()
            .Select(e => e.Entity)
            .Where(IsBaseEntity)
            .Where(e => GetDomainEvents(e).Any())
            .ToList();

        var domainEvents = entities
            .SelectMany(GetDomainEvents)
            .ToList();

        entities.ForEach(ClearDomainEvents);

        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent);
        }
    }

    private static bool IsBaseEntity(object entity)
    {
        var type = entity.GetType();

        while (type is not null)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(BaseEntity<>))
            {
                return true;
            }

            type = type.BaseType;
        }

        return false;
    }

    private static IReadOnlyCollection<BaseEvent> GetDomainEvents(object entity)
    {
        var property = entity.GetType().GetProperty(nameof(BaseEntity<int>.DomainEvents));
        return property?.GetValue(entity) as IReadOnlyCollection<BaseEvent> ?? Array.Empty<BaseEvent>();
    }

    private static void ClearDomainEvents(object entity)
    {
        var method = entity.GetType().GetMethod(nameof(BaseEntity<int>.ClearDomainEvents));
        method?.Invoke(entity, null);
    }
}
