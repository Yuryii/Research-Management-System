using RMS.Application.Application.Dtos;
using RMS.Application.Common.Exceptions;
using RMS.Application.Common.Interfaces;
using RMS.Application.Common.Models;
using RMS.Domain.Entities.Models;
using DomainApplication = RMS.Domain.Entities.Models.Application;

namespace RMS.Application.Application.Queries.GetApplications;

public class GetApplicationsQueryHandler : IRequestHandler<GetApplicationsQuery, PaginatedResult<ApplicationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;

    public GetApplicationsQueryHandler(IApplicationDbContext context, IMapper mapper, IUser user, IIdentityService identityService)
    {
        _context = context;
        _mapper = mapper;
        _user = user;
        _identityService = identityService;
    }

    public async Task<PaginatedResult<ApplicationDto>> Handle(GetApplicationsQuery request, CancellationToken cancellationToken)
    {
        IQueryable<DomainApplication> query = _context.Applications;

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        var stepId = request.StepId ?? Guid.Empty;

        if (stepId == Guid.Empty)
        {
            if (_user.Roles is null || _user.Roles.Count == 0)
            {
                throw new ForbiddenAccessException("User does not have any roles assigned.");
            }

            var roleIds = await _identityService.GetRoleIdsAsync(_user.Roles, cancellationToken);
            stepId = await _context.RoleStepPermissions
                .Where(x => roleIds.Contains(x.RoleId))
                .OrderBy(x => x.Step.Order)
                .Select(x => x.StepId)
                .FirstOrDefaultAsync(cancellationToken);

            if (stepId == Guid.Empty)
            {
                throw new ForbiddenAccessException("No step is available for the current user roles.");
            }
        }
        // Get my attachments for current step
        var currentStepAttachments = await _context.ApplicationFiles
            .Where(x => x.StepId == stepId)
            .Select(x => new
            {
                x.ApplicationId,
                File = new FileDto
                {
                    Id = x.File.Id,
                    Name = x.File.Name,
                    Path = x.File.Path,
                    ContentType = x.File.ContentType,
                    Length = x.File.Length
                }
            })
            .ToListAsync(cancellationToken);

        var currentStepAttachmentsByApplication = currentStepAttachments
            .GroupBy(x => x.ApplicationId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.File).ToList());

        // Get attachments for the previous step
        var stepOrder = await _context.StepDetails
            .Where(x => x.StepId == stepId).Select(x => x.Step.Order).FirstOrDefaultAsync(cancellationToken);

        var preStep = await _context.Steps
            .Where(x => x.Order == stepOrder - 1).FirstOrDefaultAsync(cancellationToken);

        Dictionary<Guid, List<FileDto>> preStepAttachmentsByApplication = new();

        if (preStep is not null)
        {
            var preStepAttachments = await _context.ApplicationFiles
                .Where(x => x.StepId == preStep.Id)
                .Select(x => new
                {
                    x.ApplicationId,
                    File = new FileDto
                    {
                        Id = x.File.Id,
                        Name = x.File.Name,
                        Path = x.File.Path,
                        ContentType = x.File.ContentType,
                        Length = x.File.Length
                    }
                })
                .ToListAsync(cancellationToken);

            preStepAttachmentsByApplication = preStepAttachments
                .GroupBy(x => x.ApplicationId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.File).ToList());
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.Id)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .AsNoTracking()
            .ProjectTo<ApplicationDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        var result = new PaginatedResult<ApplicationDto>(items, totalCount, request.PageNumber, request.PageSize);

        foreach (var item in result.Items)
        {
            if (preStepAttachmentsByApplication.TryGetValue(item.Id, out var preFiles))
            {
                item.PreAttachments.AddRange(preFiles);
            }

            if (currentStepAttachmentsByApplication.TryGetValue(item.Id, out var currentFiles))
            {
                item.MyApplications.AddRange(currentFiles);
            }
        }

        return result;
    }
}
