namespace RMS.Application.Steps.Commands.CreateStep;

public class CreateStepCommandValidator : AbstractValidator<CreateStepCommand>
{
    public CreateStepCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.ShortName)
            .MaximumLength(200).WithMessage("Short name must not exceed 200 characters.");

        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(0).WithMessage("Order must be zero or greater.");
    }
}
