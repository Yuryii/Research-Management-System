using RMS.Application.Common.Interfaces;
using RMS.Application.Common.Security;
using RMS.Domain.Constants;

namespace RMS.Application.Steps.Commands.DeleteStep;

[Authorize(Roles = Roles.Administrator)]
public record DeleteStepCommand(Guid Id) : IRequest;

public class DeleteStepCommandHandler : IRequestHandler<DeleteStepCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteStepCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteStepCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Steps
            .Include(s => s.StepDetails)
            .SingleOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        Guard.Against.NotFound(request.Id, entity, "Step not found.");

        _context.Steps.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
