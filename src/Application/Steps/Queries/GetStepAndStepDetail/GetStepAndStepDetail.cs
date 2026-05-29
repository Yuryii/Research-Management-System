using RMS.Application.Common.Interfaces;
using RMS.Application.Steps.Dtos;

namespace RMS.Application.Steps.Queries.GetStepAndStepDetail;

public record GetStepAndStepDetailQuery : IRequest<IList<StepDto>>
{
}

public class GetStepAndStepDetailQueryHandler : IRequestHandler<GetStepAndStepDetailQuery, IList<StepDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetStepAndStepDetailQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IList<StepDto>> Handle(GetStepAndStepDetailQuery request, CancellationToken cancellationToken)
    {
        var steps = await _context.Steps
            .Include(s => s.StepDetails.OrderBy(sd => sd.Order))
            .ToListAsync(cancellationToken);

        steps = steps.OrderBy(s => s.Order).ToList();

        return _mapper.Map<IList<StepDto>>(steps);
    }
}
