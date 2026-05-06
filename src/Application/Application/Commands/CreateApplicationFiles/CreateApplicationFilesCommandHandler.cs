using RMS.Application.Common.Exceptions;
using RMS.Application.Common.Interfaces;
using RMS.Domain.Constants;
using RMS.Domain.Entities.Models;
using RMS.Domain.Enums;

namespace RMS.Application.Application.Commands.CreateApplicationFiles;

public class CreateApplicationFilesCommandHandler : IRequestHandler<CreateApplicationFilesCommand, IReadOnlyList<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileService _fileService;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;
    public CreateApplicationFilesCommandHandler(
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

    public async Task<IReadOnlyList<Guid>> Handle(CreateApplicationFilesCommand request, CancellationToken cancellationToken)
    {
        var application = await _context.Applications
            .Include(a => a.StepDetail)
            .ThenInclude(sd => sd.Step)
            .SingleOrDefaultAsync(a => a.Id == request.ApplicationId, cancellationToken);

        Guard.Against.NotFound(request.ApplicationId, application, "Application not found.");
        Guard.Against.NotFound(request.ApplicationId, application.StepDetail, "Step detail not found.");
        Guard.Against.NotFound(request.ApplicationId, application.StepDetail.Step, "Step not found.");

        if (_user.Roles?.Contains(Roles.Teacher) == true && application.Status != ApplicationStatus.Draft)
        {
            throw new ForbiddenAccessException("Teacher can only upload files when application is in Draft status.");
        }

        var stepId = application.StepDetail.StepId;

        var currentRoleNames = _user.Roles ?? [];
        var currentRoleIds = await _identityService.GetRoleIdsAsync(currentRoleNames, cancellationToken);

        var canUpdateStep = currentRoleIds.Count > 0 && await _context.RoleStepPermissions
            .AsNoTracking()
            .AnyAsync(permission =>
                permission.StepId == stepId &&
                currentRoleIds.Contains(permission.RoleId), cancellationToken);

        if (!canUpdateStep)
        {
            throw new ForbiddenAccessException("Current role is not permitted to upload files for this step.");
        }


        var savedFiles = await _fileService.SaveFilesAsync(request.Files, cancellationToken, Config.Store.APPLICATION_PATH);
        var fileIds = new List<Guid>();
         
        foreach (var file in savedFiles)
        {
            _context.ApplicationFiles.Add(new ApplicationFile
            {
                ApplicationId = request.ApplicationId,
                FileId = file.Id,
                StepId = stepId
            });

            fileIds.Add(file.Id);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return fileIds;
    }
}
