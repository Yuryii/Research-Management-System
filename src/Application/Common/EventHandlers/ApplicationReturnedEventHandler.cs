using MediatR;
using RMS.Application.Common.Interfaces;
using RMS.Domain.Entities;
using RMS.Domain.Events;

namespace RMS.Application.Common.EventHandlers;

public class ApplicationReturnedEventHandler : INotificationHandler<ApplicationReturnedEvent>
{
    private readonly IApplicationDbContext _context;

    public ApplicationReturnedEventHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ApplicationReturnedEvent notification, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(notification.RecipientId))
        {
            return;
        }

        var entity = new Notification
        {
            Id = Guid.NewGuid(),
            Type = NotificationType.ApplicationReturned,
            Title = "Hồ sơ đã được trả về",
            Body = $"{notification.ApplicationCode}: {notification.Title}",
            RelatedApplicationId = notification.ApplicationId,
        };

        entity.Recipients.Add(new NotificationRecipient
        {
            NotificationId = entity.Id,
            UserId = notification.RecipientId,
            IsRead = false,
        });

        _context.Notifications.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
