namespace RMS.Application.Application.Commands.CreateApplicationFiles;

public class CreateApplicationFilesCommandValidator : AbstractValidator<CreateApplicationFilesCommand>
{
    public CreateApplicationFilesCommandValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty()
            .WithMessage("Application Id is required.");

        RuleFor(x => x.Files)
            .NotNull()
            .WithMessage("Files are required.")
            .Must(files => files.Count > 0)
            .WithMessage("At least one file is required.");
    }
}
