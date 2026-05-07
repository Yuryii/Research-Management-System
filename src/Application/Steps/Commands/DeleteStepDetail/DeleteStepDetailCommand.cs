using RMS.Application.Common.Interfaces;
using RMS.Application.Common.Security;
using RMS.Domain.Constants;

namespace RMS.Application.Steps.Commands.DeleteStepDetail;

[Authorize(Roles = Roles.Administrator)]
public record DeleteStepDetailCommand(Guid Id) : IRequest;

public class DeleteStepDetailCommandHandler : IRequestHandler<DeleteStepDetailCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteStepDetailCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteStepDetailCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.StepDetails
            .SingleOrDefaultAsync(sd => sd.Id == request.Id, cancellationToken);

        Guard.Against.NotFound(request.Id, entity, "Step detail not found.");

        _context.StepDetails.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
