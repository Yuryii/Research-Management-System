using RMS.Application.Common.Interfaces;
using RMS.Application.Common.Security;
using RMS.Domain.Constants;
using RMS.Domain.Entities;
using RMS.Domain.Entities.Models;

namespace RMS.Application.Application.Commands.ReturnApplication;

[Authorize(Roles = $"{Roles.Administrator}, {Roles.Tttv}, {Roles.Dvqltt}, {Roles.KhcnHtqt}")]
public record ReturnApplicationCommand : IRequest<Guid>
{
    public Guid ApplicationId { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public IFormFileCollection Files { get; init; } = null!;
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

        IReadOnlyList<string> savedFilePaths = [];
        if (request.Files.Count > 0)
        {
            var folders = $"{Config.Store.ROOT_PATH}/{Config.Store.APPLICATION_PATH}";
            savedFilePaths = await _fileService.SaveFilesAsync(
                request.Files,
                Config.Store.AllowedMimeTypes,
                folders,
                cancellationToken);

            for (var index = 0; index < request.Files.Count; index++)
            {
                var file = request.Files[index];
                var savedFilePath = savedFilePaths[index];

                _context.NotificationFiles.Add(new NotificationFile
                {
                    NotificationId = notification.Id,
                    File = new RMS.Domain.Entities.Models.File
                    {
                        Name = file.FileName,
                        ContentType = file.ContentType,
                        Length = file.Length,
                        Path = savedFilePath
                    }
                });
            }
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            foreach (var filePath in savedFilePaths)
            {
                _fileService.DeleteFile(filePath, cancellationToken);
            }

            throw;
        }

        return notification.Id;
    }
}
