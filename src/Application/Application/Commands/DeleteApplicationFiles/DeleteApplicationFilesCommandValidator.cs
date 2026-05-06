namespace RMS.Application.Application.Commands.DeleteApplicationFiles;

public class DeleteApplicationFilesCommandValidator : AbstractValidator<DeleteApplicationFilesCommand>
{
    public DeleteApplicationFilesCommandValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty()
            .WithMessage("Application Id is required.");

        RuleFor(x => x.FileId)
            .NotEmpty()
            .WithMessage("File Id is required.");
    }
}
