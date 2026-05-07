namespace RMS.Application.Steps.Commands.UpdateStep;

public class UpdateStepCommandValidator : AbstractValidator<UpdateStepCommand>
{
    public UpdateStepCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Step Id is required.");

        RuleFor(x => x.Name)
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.ShortName)
            .MaximumLength(200).WithMessage("Short name must not exceed 200 characters.");

        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(0).WithMessage("Order must be zero or greater.");
    }
}
