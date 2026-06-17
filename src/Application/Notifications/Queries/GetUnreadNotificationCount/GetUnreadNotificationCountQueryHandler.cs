using Microsoft.EntityFrameworkCore;
using RMS.Application.Common.Interfaces;

namespace RMS.Application.Notifications.Queries.GetUnreadNotificationCount;

public class GetUnreadNotificationCountQueryHandler : IRequestHandler<GetUnreadNotificationCountQuery, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public GetUnreadNotificationCountQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<int> Handle(GetUnreadNotificationCountQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_user.Id))
        {
            return 0;
        }

        return await _context.NotificationRecipients
            .AsNoTracking()
            .Where(r => r.UserId == _user.Id && !r.IsRead)
            .CountAsync(cancellationToken);
    }
}
