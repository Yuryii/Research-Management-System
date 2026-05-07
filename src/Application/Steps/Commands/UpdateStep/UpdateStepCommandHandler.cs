using RMS.Application.Common.Interfaces;

namespace RMS.Application.Steps.Commands.UpdateStep;

public class UpdateStepCommandHandler : IRequestHandler<UpdateStepCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateStepCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateStepCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Steps
            .FindAsync([request.Id], cancellationToken);

        Guard.Against.NotFound(request.Id, entity, "Step not found.");

        if (request.Name is not null)
        {
            entity.Name = request.Name;
        }

        if (request.ShortName is not null)
        {
            entity.ShortName = request.ShortName;
        }

        if (request.Order.HasValue)
        {
            entity.Order = request.Order.Value;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
