namespace RMS.Application.Application.Commands.UpdateApplicationStepDetail;

public class UpdateApplicationStepDetailCommandValidator : AbstractValidator<UpdateApplicationStepDetailCommand>
{
    public UpdateApplicationStepDetailCommandValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty()
            .WithMessage("Application Id is required.");

        RuleFor(x => x.StepDetailId)
            .NotEmpty()
            .WithMessage("Step Detail Id is required.");
    }
}
