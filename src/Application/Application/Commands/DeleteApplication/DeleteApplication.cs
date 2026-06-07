using Microsoft.EntityFrameworkCore;
using RMS.Application.Common.Exceptions;
using RMS.Application.Common.Interfaces;
using RMS.Domain.Enums;

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

        if (entity.Status != ApplicationStatus.Draft)
            throw new ForbiddenAccessException("Only applications in Draft status can be deleted.");

        foreach (var item in entity.ApplicationFiles)
        {
            _fileService.DeleteFile(item.File.Path);
        }

        _context.Applications.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
