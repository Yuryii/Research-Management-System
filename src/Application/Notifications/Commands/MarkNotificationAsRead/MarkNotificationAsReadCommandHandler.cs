using Microsoft.EntityFrameworkCore;
using RMS.Application.Common.Exceptions;
using RMS.Application.Common.Interfaces;
using RMS.Domain.Entities;

namespace RMS.Application.Notifications.Commands.MarkNotificationAsRead;

public class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public MarkNotificationAsReadCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<Unit> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_user.Id))
        {
            throw new ForbiddenAccessException();
        }

        var recipient = await _context.NotificationRecipients
            .FirstOrDefaultAsync(r => r.NotificationId == request.Id && r.UserId == _user.Id, cancellationToken);

        if (recipient is null)
        {
            throw new NotFoundException(request.Id.ToString(), nameof(NotificationRecipient));
        }

        if (!recipient.IsRead)
        {
            recipient.IsRead = true;
            recipient.ReadAt = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}
