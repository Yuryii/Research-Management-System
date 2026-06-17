using FluentValidation;
using RMS.Application.Notifications.Queries.GetMyNotifications;

namespace RMS.Application.Notifications.Queries.GetMyNotifications;

public class GetMyNotificationsQueryValidator : AbstractValidator<GetMyNotificationsQuery>
{
    public GetMyNotificationsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100);
    }
}
