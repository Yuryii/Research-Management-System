using FluentValidation;

namespace RMS.Application.Application.Commands.DeleteApplication;

public class DeleteApplicationCommandValidator : AbstractValidator<DeleteApplicationCommand>
{
    public DeleteApplicationCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Application Id is required.");
    }
}
