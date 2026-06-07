using FluentValidation;

namespace RMS.Application.Application.Commands.ReturnApplication;

public class ReturnApplicationCommandValidator : AbstractValidator<ReturnApplicationCommand>
{
    public ReturnApplicationCommandValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty().WithMessage("Application ID is required.");
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.");
        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required.");
    }
}
