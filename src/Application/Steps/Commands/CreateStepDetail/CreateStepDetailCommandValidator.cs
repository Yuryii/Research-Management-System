namespace RMS.Application.Steps.Commands.CreateStepDetail;

public class CreateStepDetailCommandValidator : AbstractValidator<CreateStepDetailCommand>
{
    public CreateStepDetailCommandValidator()
    {
        RuleFor(x => x.StepId)
            .NotEmpty().WithMessage("Step Id is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(0).WithMessage("Order must be zero or greater.");

        RuleFor(x => x.NextStepDetailId)
            .NotEqual(x => x.StepId)
            .When(x => x.NextStepDetailId.HasValue)
            .WithMessage("Next step detail cannot match the step id.");
        RuleFor(x => x.IsReturnStep)
            .Must((command, isReturnStep) =>
            {
                if (isReturnStep)
                {
                    return !command.NextStepDetailId.HasValue;
                }
                return true;
            })
            .WithMessage("Return steps cannot have a next step detail.");
    }
}
