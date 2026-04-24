using System;
using System.Collections.Generic;
using System.Text;
using RMS.Domain.Enums;

namespace RMS.Application.Application.Commands.UpdateApplication;

public class UpdateApplicationCommandValidator : AbstractValidator<UpdateApplicationCommand>
{
    public UpdateApplicationCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Application Id is required.");
        RuleFor(x => x.Title)
            .MaximumLength(100)
            .WithMessage("Title cannot exceed 100 characters.");
        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters.");
        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Invalid application status.")
            .Equal(ApplicationStatus.Draft)
            .WithMessage("Only applications in Draft status can be updated.");
    }
}
