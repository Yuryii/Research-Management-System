using MediatR;

namespace RMS.Application.Notifications.Queries.GetUnreadNotificationCount;

public record GetUnreadNotificationCountQuery : IRequest<int>;
