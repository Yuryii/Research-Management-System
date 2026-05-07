using RMS.Application.Common.Exceptions;
using RMS.Application.Common.Interfaces;

namespace RMS.Application.Application.Commands.UpdateApplicationStepDetail;

public class UpdateApplicationStepDetailCommandHandler : IRequestHandler<UpdateApplicationStepDetailCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;

    public UpdateApplicationStepDetailCommandHandler(
        IApplicationDbContext context,
        IUser user,
        IIdentityService identityService)
    {
        _context = context;
        _user = user;
        _identityService = identityService;
    }

    public async Task Handle(UpdateApplicationStepDetailCommand request, CancellationToken cancellationToken)
    {
        var application = await _context.Applications
            .FindAsync([request.ApplicationId], cancellationToken);

        Guard.Against.NotFound(request.ApplicationId, application, "Application not found.");

        var isReturnStep = await _context.StepDetails
            .AsNoTracking()
            .AnyAsync(sd => sd.Id == application.StepDetailId && sd.IsReturnStep, cancellationToken);

        if (isReturnStep)
        {
            throw new InvalidOperationException("Cannot update step detail for application in return step.");
        }

        var stepDetail = await _context.StepDetails
            .AsNoTracking()
            .SingleOrDefaultAsync(sd => sd.Id == request.StepDetailId, cancellationToken);

        Guard.Against.NotFound(request.StepDetailId, stepDetail, "Step detail not found.");

        var currentRoleNames = _user.Roles ?? [];
        var currentRoleIds = await _identityService.GetRoleIdsAsync(currentRoleNames, cancellationToken);

        bool canUpdateStepDetail = currentRoleIds.Count > 0 && await _context.RoleStepPermissions
            .AsNoTracking() 
            .AnyAsync(permission =>
                permission.StepId == stepDetail.StepId &&
                currentRoleIds.Contains(permission.RoleId), cancellationToken);

        if (!canUpdateStepDetail)
        {
            throw new ForbiddenAccessException();
        }

        application.StepDetailId = request.StepDetailId;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
