using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RMS.Application.Common.Exceptions;
using RMS.Application.Common.Interfaces;
using RMS.Domain.Constants;
using RMS.Domain.Entities.Models;
using RMS.Domain.Enums;

namespace RMS.Application.Application.Commands.CreateApplicationFiles;

public class CreateApplicationFilesCommandHandler : IRequestHandler<CreateApplicationFilesCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IApplicationFileService _applicationFileService;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;

    public CreateApplicationFilesCommandHandler(
        IApplicationDbContext context,
        IApplicationFileService applicationFileService,
        IUser user,
        IIdentityService identityService)
    {
        _context = context;
        _applicationFileService = applicationFileService;
        _user = user;
        _identityService = identityService;
    }

    public async Task Handle(CreateApplicationFilesCommand request, CancellationToken cancellationToken)
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

        await _applicationFileService.AddFilesToApplicationAsync(
            request.ApplicationId,
            stepId,
            request.Files,
            cancellationToken);
    }
}
