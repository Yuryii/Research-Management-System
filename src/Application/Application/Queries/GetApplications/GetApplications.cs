using RMS.Application.Application.Dtos;
using RMS.Application.Common.Interfaces;
using RMS.Application.Common.Models;
using RMS.Application.Common.Extensions;
using RMS.Domain.Constants;
using RMS.Domain.Entities.Models;
using DomainApplication = RMS.Domain.Entities.Models.Application;

namespace RMS.Application.Application.Queries.GetApplications;

public class GetApplicationsQueryHandler : IRequestHandler<GetApplicationsQuery, PaginatedResult<ApplicationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;
    private readonly IApplicationQueryService _queryService;

    public GetApplicationsQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        IUser user,
        IIdentityService identityService,
        IApplicationQueryService queryService)
    {
        _context = context;
        _mapper = mapper;
        _user = user;
        _identityService = identityService;
        _queryService = queryService;
    }

    public async Task<PaginatedResult<ApplicationDto>> Handle(GetApplicationsQuery request, CancellationToken cancellationToken)
    {
        IQueryable<DomainApplication> query = _context.Applications;

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchLower = request.Search.Trim().ToLower();
            query = query.Where(x =>
                x.Title.ToLower().Contains(searchLower) ||
                x.Description.ToLower().Contains(searchLower));
        }

        bool isTeacher = _user.Roles?.Contains(Roles.Teacher) == true && !string.IsNullOrEmpty(_user.Id);

        var stepContext = await _queryService.ResolveStepContextAsync(
            _user.Roles ?? new List<string>(),
            request.StepDetailId,
            cancellationToken);

        if (!isTeacher)
        {
            if (request.StepId.HasValue)
            {
                query = query.Where(x => x.StepDetail.StepId == request.StepId.Value);
            }
            else if (request.StepDetailId.HasValue)
            {
                query = query.Where(x => x.StepDetailId == request.StepDetailId);
            }
            else
            {
                query = query.Where(x => x.StepDetailId == stepContext.StepDetailId);
            }
        }

        if (isTeacher)
        {
            query = query.Where(x => x.CreatedBy == _user.Id);
        }

        var result = await query.ToPaginatedResultAsync<DomainApplication, ApplicationDto, Guid>(
            request, x => x.Id, _mapper.ConfigurationProvider, cancellationToken);

        foreach (var item in result.Items)
        {
            if (stepContext.PreviousStepAttachments.TryGetValue(item.Id, out var preFiles))
                item.PreAttachments.AddRange(preFiles);

            if (stepContext.CurrentStepAttachments.TryGetValue(item.Id, out var currentFiles))
                item.MyApplications.AddRange(currentFiles);

            if (!string.IsNullOrEmpty(item.CreatedBy))
                item.TeacherName = await _identityService.GetFullNameAsync(item.CreatedBy);
        }

        return result;
    }
}
