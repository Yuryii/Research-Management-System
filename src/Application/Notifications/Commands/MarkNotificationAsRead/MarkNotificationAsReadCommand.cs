using MediatR;

namespace RMS.Application.Notifications.Commands.MarkNotificationAsRead;

public record MarkNotificationAsReadCommand : IRequest<Unit>
{
    public Guid Id { get; init; }
}
