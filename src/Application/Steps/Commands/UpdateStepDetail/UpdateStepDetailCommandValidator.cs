namespace RMS.Application.Steps.Commands.UpdateStepDetail;

public class UpdateStepDetailCommandValidator : AbstractValidator<UpdateStepDetailCommand>
{
    public UpdateStepDetailCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Step detail Id is required.");

        RuleFor(x => x.Name)
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(0).WithMessage("Order must be zero or greater.");
    }
}
