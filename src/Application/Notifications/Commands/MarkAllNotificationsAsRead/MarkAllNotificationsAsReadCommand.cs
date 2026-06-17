using MediatR;

namespace RMS.Application.Notifications.Commands.MarkAllNotificationsAsRead;

public record MarkAllNotificationsAsReadCommand : IRequest<int>;
