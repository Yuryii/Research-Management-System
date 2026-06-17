using MediatR;
using Microsoft.EntityFrameworkCore;
using RMS.Application.Common.Interfaces;
using RMS.Domain.Entities;
using RMS.Domain.Events;

namespace RMS.Application.Common.EventHandlers;

public class ApplicationForwardedNotificationHandler : INotificationHandler<ApplicationForwardedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public ApplicationForwardedNotificationHandler(IApplicationDbContext context, IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    public async Task Handle(ApplicationForwardedEvent notification, CancellationToken cancellationToken)
    {
        // Resolve roles for the step the application was forwarded TO. We use the
        // explicit NextStepId from the event payload (not the application's current
        // StepDetail) so we don't depend on the order in which state mutations and
        // domain-event dispatch happen.
        var roleIds = await _context.RoleStepPermissions
            .AsNoTracking()
            .Where(rsp => rsp.StepId == notification.NextStepId)
            .Select(rsp => rsp.RoleId)
            .ToListAsync(cancellationToken);

        if (roleIds.Count == 0)
        {
            return;
        }

        var userIds = await _identityService.GetUserIdsInRoleIdsAsync(roleIds, cancellationToken);
        if (userIds.Count == 0)
        {
            return;
        }

        var entity = new Notification
        {
            Id = Guid.NewGuid(),
            Type = NotificationType.ApplicationForwarded,
            Title = "Hồ sơ được chuyển tiếp",
            Body = $"Hồ sơ {notification.ApplicationCode} đã chuyển tiếp từ \"{notification.FromStepName}\" đến \"{notification.ToStepName}\".",
            RelatedApplicationId = notification.ApplicationId,
        };

        foreach (var userId in userIds)
        {
            entity.Recipients.Add(new NotificationRecipient
            {
                NotificationId = entity.Id,
                UserId = userId,
                IsRead = false,
            });
        }

        _context.Notifications.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
