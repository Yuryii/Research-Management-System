using System;
using System.Collections.Generic;
using System.Text;

namespace RMS.Application.Application.Commands.ForwardNextToStep;

public class ForwardNextToStepCommandValidator : AbstractValidator<ForwardNextToStepCommand>
{
    public ForwardNextToStepCommandValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty().WithMessage("Application ID is required.")
            .Must(id => id != Guid.Empty).WithMessage("Application ID cannot be an empty GUID.");
    }
}
