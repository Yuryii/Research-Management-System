using RMS.Application.Common.Interfaces;

namespace RMS.Application.Steps.Commands.UpdateStepDetail;

public class UpdateStepDetailCommandHandler : IRequestHandler<UpdateStepDetailCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateStepDetailCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateStepDetailCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.StepDetails
            .SingleOrDefaultAsync(sd => sd.Id == request.Id, cancellationToken);

        Guard.Against.NotFound(request.Id, entity, "Step detail not found.");

        if (request.Name is not null)
        {
            entity.Name = request.Name;
        }

        if (request.Order.HasValue)
        {
            entity.Order = request.Order.Value;
        }

        if (request.NextStepDetailId.HasValue)
        {
            var nextStepDetail = await _context.StepDetails
                .SingleOrDefaultAsync(sd => sd.Id == request.NextStepDetailId.Value, cancellationToken);

            Guard.Against.NotFound(request.NextStepDetailId.Value, nextStepDetail, "Next step detail not found.");

            entity.NextStepDetailId = request.NextStepDetailId;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
