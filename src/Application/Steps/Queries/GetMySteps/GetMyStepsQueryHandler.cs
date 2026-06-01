using Microsoft.EntityFrameworkCore;
using RMS.Application.Common.Interfaces;
using RMS.Application.Steps.Dtos;

namespace RMS.Application.Steps.Queries.GetMySteps;

public class GetMyStepsQueryHandler : IRequestHandler<GetMyStepsQuery, IList<StepDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;

    public GetMyStepsQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        IUser user,
        IIdentityService identityService)
    {
        _context = context;
        _mapper = mapper;
        _user = user;
        _identityService = identityService;
    }

    public async Task<IList<StepDto>> Handle(GetMyStepsQuery request, CancellationToken cancellationToken)
    {
        if (_user.Roles is null || _user.Roles.Count == 0)
        {
            return new List<StepDto>();
        }

        var roleIds = await _identityService.GetRoleIdsAsync(_user.Roles, cancellationToken);

        var steps = await _context.RoleStepPermissions
            .Where(x => roleIds.Contains(x.RoleId))
            .Include(x => x.Step)
                .ThenInclude(s => s.StepDetails.OrderBy(sd => sd.Order))
            .Select(x => x.Step)
            .Where(s => s != null)
            .Distinct()
            .OrderBy(s => s!.Order)
            .ToListAsync(cancellationToken);

        return _mapper.Map<IList<StepDto>>(steps!);
    }
}
