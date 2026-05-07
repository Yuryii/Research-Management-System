using RMS.Application.Common.Interfaces;
using RMS.Domain.Entities.Models;

namespace RMS.Application.Steps.Commands.CreateStepDetail;

public class CreateStepDetailCommandHandler : IRequestHandler<CreateStepDetailCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateStepDetailCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateStepDetailCommand request, CancellationToken cancellationToken)
    {
        var step = await _context.Steps
            .SingleOrDefaultAsync(s => s.Id == request.StepId, cancellationToken);

        Guard.Against.NotFound(request.StepId, step, "Step not found.");

        StepDetail? nextStepDetail = null;

        if (request.NextStepDetailId.HasValue)
        {
            nextStepDetail = await _context.StepDetails
                .SingleOrDefaultAsync(sd => sd.Id == request.NextStepDetailId.Value, cancellationToken);

            Guard.Against.NotFound(request.NextStepDetailId.Value, nextStepDetail, "Next step detail not found.");
        }

        var entity = new StepDetail
        {
            Id = Guid.NewGuid(),
            StepId = request.StepId,
            Name = request.Name,
            Order = request.Order,
            NextStepDetailId = request.NextStepDetailId,
            IsReturnStep = request.IsReturnStep
        };

        _context.StepDetails.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
