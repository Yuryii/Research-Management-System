using Microsoft.EntityFrameworkCore;
using RMS.Application.Common.Extensions;
using RMS.Application.Common.Interfaces;
using RMS.Application.Common.Models;
using RMS.Application.Notifications.Dtos;
using RMS.Domain.Entities;

namespace RMS.Application.Notifications.Queries.GetMyNotifications;

public class GetMyNotificationsQueryHandler : IRequestHandler<GetMyNotificationsQuery, PaginatedResult<NotificationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IMapper _mapper;

    public GetMyNotificationsQueryHandler(
        IApplicationDbContext context,
        IUser user,
        IMapper mapper)
    {
        _context = context;
        _user = user;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<NotificationDto>> Handle(GetMyNotificationsQuery request, CancellationToken cancellationToken)
    {
        IQueryable<NotificationRecipient> query = _context.NotificationRecipients
            .AsNoTracking()
            .Where(r => r.UserId == _user.Id);

        var result = await query.ToPaginatedResultAsync<NotificationRecipient, NotificationDto, DateTimeOffset>(
            request, r => r.Notification.Created, _mapper.ConfigurationProvider, cancellationToken);

        return result;
    }
}
