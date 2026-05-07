using RMS.Application.Common.Interfaces;

namespace RMS.Application.Steps.Queries.GetStepAndStepDetail;

public record GetStepAndStepDetailQuery : IRequest<StepDto>
{
    public Guid StepDetailId { get; init; }
}

public class GetStepAndStepDetailQueryValidator : AbstractValidator<GetStepAndStepDetailQuery>
{
    public GetStepAndStepDetailQueryValidator()
    {
        RuleFor(x => x.StepDetailId)
            .NotEmpty();
    }
}

public class GetStepAndStepDetailQueryHandler : IRequestHandler<GetStepAndStepDetailQuery, StepDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetStepAndStepDetailQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<StepDto> Handle(GetStepAndStepDetailQuery request, CancellationToken cancellationToken)
    {
        var stepDetail = await _context.StepDetails
            .Include(sd => sd.Step)
            .ThenInclude(s => s.StepDetails)
            .SingleOrDefaultAsync(sd => sd.Id == request.StepDetailId, cancellationToken);

        Guard.Against.NotFound(request.StepDetailId, stepDetail, "Step detail not found.");
        Guard.Against.NotFound(request.StepDetailId, stepDetail.Step, "Step not found.");

        stepDetail.Step.StepDetails = stepDetail.Step.StepDetails
            .OrderBy(sd => sd.Order)
            .ToList();

        return _mapper.Map<StepDto>(stepDetail.Step);
    }
}
