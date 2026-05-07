using RMS.Application.Common.Interfaces;
using RMS.Application.Common.Models;
using RMS.Application.Common.Security;
using RMS.Domain.Constants;
using RMS.Domain.Entities;
using RMS.Domain.Entities.Models;

namespace RMS.Application.Application.Commands.ReturnApplication;

[Authorize(Roles = Roles.Administrator)]
public record ReturnApplicationCommand : IRequest<Guid>
{
    public Guid ApplicationId { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public IReadOnlyList<FileUploadDto> Files { get; init; } = [];
}

public class ReturnApplicationCommandValidator : AbstractValidator<ReturnApplicationCommand>
{
    public ReturnApplicationCommandValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty();
        RuleFor(x => x.Title)
            .NotEmpty();
        RuleFor(x => x.Description)
            .NotEmpty();
    }
}

public class ReturnApplicationCommandHandler : IRequestHandler<ReturnApplicationCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileService _fileService;

    public ReturnApplicationCommandHandler(IApplicationDbContext context, IFileService fileService)
    {
        _context = context;
        _fileService = fileService;
    }

    public async Task<Guid> Handle(ReturnApplicationCommand request, CancellationToken cancellationToken)
    {
        var application = await _context.Applications
            .SingleOrDefaultAsync(a => a.Id == request.ApplicationId, cancellationToken);

        Guard.Against.NotFound(request.ApplicationId, application, "Application not found.");

        var returnStepDetail = await _context.StepDetails
            .AsNoTracking()
            .SingleOrDefaultAsync(sd => sd.IsReturnStep, cancellationToken);

        Guard.Against.NotFound(request.ApplicationId, returnStepDetail, "Return step detail not found.");

        application.StepDetailId = returnStepDetail.Id;

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            RecipientId = application.CreatedBy
        };

        _context.Notifications.Add(notification);

        if (request.Files.Count > 0)
        {
            var savedFiles = await _fileService.SaveFilesAsync(request.Files, cancellationToken, Config.Store.APPLICATION_PATH);
            foreach (var file in savedFiles)
            {
                _context.NotificationFiles.Add(new NotificationFile
                {
                    NotificationId = notification.Id,
                    FileId = file.Id
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return notification.Id;
    }
}
