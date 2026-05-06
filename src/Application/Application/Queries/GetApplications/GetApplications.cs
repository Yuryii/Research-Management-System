using RMS.Application.Application.Dtos;
using RMS.Application.Common.Interfaces;
using RMS.Application.Common.Models;
using RMS.Domain.Entities.Models;
using DomainApplication = RMS.Domain.Entities.Models.Application;

namespace RMS.Application.Application.Queries.GetApplications;

public class GetApplicationsQueryHandler : IRequestHandler<GetApplicationsQuery, PaginatedResult<ApplicationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetApplicationsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<ApplicationDto>> Handle(GetApplicationsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Applications;
        //Gat my attachtments for current step
        var currentStepAttachments = await _context.ApplicationFiles
            .Where(x => x.StepId == request.StepId)
            .ProjectTo<ApplicationFileDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        // Get attachments for the previous step
        var stepOrder = await _context.StepDetails
            .Where(x => x.StepId == request.StepId).Select(x => x.Step.Order).FirstOrDefaultAsync(cancellationToken);

        var preStep = await _context.Steps
            .Where(x => x.Order == stepOrder - 1).FirstOrDefaultAsync(cancellationToken);

        List<ApplicationFileDto> preStepAttachments = new List<ApplicationFileDto>();

        if (preStep is not null)
        {
            preStepAttachments = await _context.ApplicationFiles
            .Where(x => x.StepId == preStep.Id)
            .ProjectTo<ApplicationFileDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
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

            foreach (var item1 in preStepAttachments)
            {
                if(item1.ApplicationId == item.Id)
                {
                    item.PreAttachments.Add(item1);
                }
            }
            foreach (var item2 in currentStepAttachments)
            {
                if (item2.ApplicationId == item.Id)
                {
                    item.MyApplications.Add(item2);
                }
            }
        }

        return result;
    }
}
