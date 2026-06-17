using FluentValidation;

namespace RMS.Application.Notifications.Commands.MarkNotificationAsRead;

public class MarkNotificationAsReadCommandValidator : AbstractValidator<MarkNotificationAsReadCommand>
{
    public MarkNotificationAsReadCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
