using RMS.Application.Common.Interfaces;

namespace RMS.Application.Application.Commands.DeleteApplication;

public record DeleteApplicationCommand(Guid Id) : IRequest;
public class DeleteApplicationCommandHandler : IRequestHandler<DeleteApplicationCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileService _fileService;

    public DeleteApplicationCommandHandler(IApplicationDbContext context, IFileService fileService)
    {
        _context = context;
        _fileService = fileService;
    }

    public async Task Handle(DeleteApplicationCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Applications
            .Include(a => a.ApplicationFiles)
            .Where(a => a.Id == request.Id)
            .SingleOrDefaultAsync(cancellationToken);

        Guard.Against.NotFound(request.Id, entity, "Application not found.");

        var fileIds = entity.ApplicationFiles
            .Select(af => af.FileId)
            .ToList();

        _context.Applications.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);

        foreach (var fileId in fileIds)
        {
            await _fileService.DeleteFileAsync(fileId, cancellationToken);
        }
    }
}
