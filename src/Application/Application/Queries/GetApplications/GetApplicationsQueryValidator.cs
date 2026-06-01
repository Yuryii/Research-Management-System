using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using RMS.Application.Common.Exceptions;
using RMS.Application.Common.Interfaces;
using RMS.Domain.Constants;

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
        RuleFor(x => x.StepDetailId)
            .MustAsync((stepDetailId, cancellationToken) => IsStepDetailRoleMatch(stepDetailId, cancellationToken))
            .When(x => x.StepDetailId.HasValue && x.StepDetailId != Guid.Empty && !IsTeacher())
            .WithMessage("Forbidden")
            .WithErrorCode("403");
        _identityService = identityService;
    }

    private bool IsTeacher() => _user.Roles?.Contains(Roles.Teacher) == true;

    internal async Task<bool> IsStepDetailRoleMatch(Guid? stepDetailId, CancellationToken cancellationToken)
    {
        if (!stepDetailId.HasValue || stepDetailId == Guid.Empty)
        {
            return true;
        }
        if (_user.Roles is not null)
        {
            var roleIds = await _identityService.GetRoleIdsAsync(_user.Roles, cancellationToken);

            return await _context.StepDetails
                .Where(sd => sd.Id == stepDetailId.Value)
                .Include(sd => sd.Step)
                .ThenInclude(s => s.RoleStepPermissions)
                .AnyAsync(sd => sd.Step.RoleStepPermissions.Any(rsp => roleIds.Contains(rsp.RoleId)), cancellationToken);
        }
        return false;
    }
}
