using RMS.Application.Common.Interfaces;
using RMS.Domain.Constants;
using RMS.Domain.Entities.Models;
using DomainApplication = RMS.Domain.Entities.Models.Application;   
namespace RMS.Application.Application.Commands.ForwardNextToStep;

public class ForwardNextToStepCommandHandler : IRequestHandler<ForwardNextToStepCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IUser _user;

    public ForwardNextToStepCommandHandler(IApplicationDbContext context, IUser user, IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
        _user = user;
    }

    public async Task<Guid> Handle(ForwardNextToStepCommand request, CancellationToken cancellationToken)
    { 
        var application = await _context.Applications.FindAsync(new object[] { request.ApplicationId }, cancellationToken);

        Guard.Against.NotFound(request.ApplicationId, application, "Application not found.");

        var isReturnStep = await _context.StepDetails
            .AsNoTracking()
            .AnyAsync(sd => sd.Id == application.StepDetailId && sd.IsReturnStep, cancellationToken);

        if (isReturnStep)
        {
            throw new InvalidOperationException("Cannot update step detail for application in return step.");
        }

        // validate only have permissions to  update application on step match with their role
        List<string> roleNames = _user.Roles!;
        var roleIds = await _identityService.GetRoleIdsAsync(roleNames, cancellationToken);

        List<Step> stepFromRoles = await  _context.RoleStepPermissions
            .Where(rsp => roleIds.Contains(rsp.RoleId))
            .Select(rsp => rsp.Step)
            .ToListAsync(cancellationToken);

        bool canUpdateApplication = stepFromRoles.Any(s => s.Id == application.StepDetail.StepId);

        if(!canUpdateApplication)
        {
            throw new UnauthorizedAccessException("User does not have permission to update the application at the current step.");
        }

        application.StepDetailId = application.StepDetail.NextStepDetailId ?? throw new InvalidOperationException("Next step detail ID is not defined.");

        await _context.SaveChangesAsync(cancellationToken);

        return application.Id;
    }
}
