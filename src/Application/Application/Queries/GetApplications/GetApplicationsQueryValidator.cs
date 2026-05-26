using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using RMS.Application.Common.Exceptions;
using RMS.Application.Common.Interfaces;

namespace RMS.Application.Application.Queries.GetApplications;

public class GetApplicationsQueryValidator : AbstractValidator<GetApplicationsQuery>
{
    private readonly IUser _user;
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public GetApplicationsQueryValidator(IUser user, IApplicationDbContext context, IIdentityService identityService)
    {
        _user = user;
        _context = context;

        RuleFor(x => x.PageNumber)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100);
        RuleFor(x => x.StepId)
            .MustAsync((stepId, cancellationToken) => IsStepRoleMatch(stepId, cancellationToken))
            .When(x => x.StepId.HasValue && x.StepId != Guid.Empty)
            .WithMessage("Forbidden")
            .WithErrorCode("403");
        _identityService = identityService;
    }

    internal async Task<bool> IsStepRoleMatch(Guid? stepId, CancellationToken cancellationToken)
    {
        if (!stepId.HasValue || stepId == Guid.Empty)
        {
            return true;
        }
        if (_user.Roles is not null)
        {
            var roleIds = await _identityService.GetRoleIdsAsync(_user.Roles, cancellationToken);
            return _user.Roles is null
                ? false
                : await _context.RoleStepPermissions
                .AnyAsync(x => x.StepId == stepId.Value && roleIds.Contains(x.RoleId));
        }
        return false;
    }
}
