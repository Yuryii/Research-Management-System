using Microsoft.EntityFrameworkCore;
using RMS.Application.Common.Exceptions;
using RMS.Application.Common.Interfaces;
using RMS.Domain.Constants;
using RMS.Domain.Enums;

namespace RMS.Application.Application.Commands.DeleteApplicationFiles;

public class DeleteApplicationFilesCommandHandler : IRequestHandler<DeleteApplicationFilesCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileService _fileService;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;

    public DeleteApplicationFilesCommandHandler(
        IApplicationDbContext context,
        IFileService fileService,
        IUser user,
        IIdentityService identityService)
    {
        _context = context;
        _fileService = fileService;
        _user = user;
        _identityService = identityService;
    }

    public async Task Handle(DeleteApplicationFilesCommand request, CancellationToken cancellationToken)
    {
        var applicationFile = await _context.ApplicationFiles
            .Include(af => af.Application)
            .Include(af => af.File)
            .SingleOrDefaultAsync(af => af.ApplicationId == request.ApplicationId && af.FileId == request.FileId, cancellationToken);

        Guard.Against.NotFound(request.FileId, applicationFile, "Application file not found.");

        if (_user.Roles?.Contains(Roles.Teacher) == true && applicationFile.Application.Status != ApplicationStatus.Draft)
        {
            throw new ForbiddenAccessException();
        }

        var currentRoleNames = _user.Roles ?? [];
        var currentRoleIds = await _identityService.GetRoleIdsAsync(currentRoleNames, cancellationToken);

        var canUpdateStep = currentRoleIds.Count > 0 && await _context.RoleStepPermissions
            .AsNoTracking()
            .AnyAsync(permission =>
                permission.StepId == applicationFile.StepId &&
                currentRoleIds.Contains(permission.RoleId), cancellationToken);

        if (!canUpdateStep)
        {
            throw new ForbiddenAccessException();
        }

        _context.ApplicationFiles.Remove(applicationFile);

        if (applicationFile.File is not null)
        {
            _context.Files.Remove(applicationFile.File);
        }

        await _context.SaveChangesAsync(cancellationToken);

        await _fileService.DeleteFileAsync(request.FileId, cancellationToken);
    }
}
