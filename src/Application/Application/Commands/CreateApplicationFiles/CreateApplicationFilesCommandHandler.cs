using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RMS.Application.Common.Exceptions;
using RMS.Application.Common.Interfaces;
using RMS.Domain.Constants;
using RMS.Domain.Entities.Models;
using RMS.Domain.Enums;
using DomainFile = RMS.Domain.Entities.Models.File;
namespace RMS.Application.Application.Commands.CreateApplicationFiles;

public class CreateApplicationFilesCommandHandler : IRequestHandler<CreateApplicationFilesCommand>
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

    public async Task Handle(CreateApplicationFilesCommand request, CancellationToken cancellationToken)
    {
        // Validate 
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

        // Logic

        var folders = $"{Config.Store.ROOT_PATH}/{Config.Store.APPLICATION_PATH}";
        var savedFilePaths = await _fileService.SaveFilesAsync(
            request.Files,
            Config.Store.AllowedMimeTypes,
            folders,
            cancellationToken);

        for (var index = 0; index < request.Files.Count; index++)
        {
            var file = request.Files[index];
            var savedFilePath = savedFilePaths[index];

            _context.ApplicationFiles.Add(new ApplicationFile
            {
                ApplicationId = request.ApplicationId,
                File = new DomainFile
                {
                    Name = file.FileName,
                    ContentType = file.ContentType,
                    Length = file.Length,
                    Path = savedFilePath
                },
                StepId = stepId
            });
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            foreach (var savedFilePath in savedFilePaths)
            {
                _fileService.DeleteFile(savedFilePath, cancellationToken);
            }

            throw;
        }
    }
}
