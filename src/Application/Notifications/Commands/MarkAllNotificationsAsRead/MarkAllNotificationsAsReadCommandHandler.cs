using Microsoft.EntityFrameworkCore;
using RMS.Application.Common.Interfaces;
using RMS.Domain.Entities;

namespace RMS.Application.Notifications.Commands.MarkAllNotificationsAsRead;

public class MarkAllNotificationsAsReadCommandHandler : IRequestHandler<MarkAllNotificationsAsReadCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public MarkAllNotificationsAsReadCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<int> Handle(MarkAllNotificationsAsReadCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_user.Id))
        {
            return 0;
        }

        var unread = await _context.NotificationRecipients
            .Where(r => r.UserId == _user.Id && !r.IsRead)
            .ToListAsync(cancellationToken);

        if (unread.Count == 0)
        {
            return 0;
        }

        var now = DateTimeOffset.UtcNow;
        foreach (var r in unread)
        {
            r.IsRead = true;
            r.ReadAt = now;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return unread.Count;
    }
}
