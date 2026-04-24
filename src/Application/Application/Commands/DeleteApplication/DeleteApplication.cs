using RMS.Application.Common.Interfaces;

namespace RMS.Application.Application.Commands.DeleteApplication;

public record DeleteApplicationCommand(Guid Id) : IRequest;
public class DeleteApplicationCommandHandler : IRequestHandler<DeleteApplicationCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteApplicationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteApplicationCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Applications
            .Where(a => a.Id == request.Id)
            .SingleOrDefaultAsync(cancellationToken);

        Guard.Against.NotFound(request.Id, entity, "Application not found.");

        _context.Applications.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
