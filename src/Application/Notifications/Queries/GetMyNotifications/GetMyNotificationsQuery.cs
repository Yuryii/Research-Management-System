using RMS.Application.Common.Models;
using RMS.Application.Notifications.Dtos;

namespace RMS.Application.Notifications.Queries.GetMyNotifications;

public record GetMyNotificationsQuery : PagedQuery<NotificationDto>
{
}
