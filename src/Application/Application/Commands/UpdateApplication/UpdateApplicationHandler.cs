using RMS.Application.Common.Interfaces;
using DomainApplication = RMS.Domain.Entities.Models.Application;

namespace RMS.Application.Application.Commands.UpdateApplication;

public class UpdateApplicationCommandHandler : IRequestHandler<UpdateApplicationCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateApplicationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateApplicationCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Applications
            .FindAsync([request.Id], cancellationToken);

        Guard.Against.NotFound(request.Id, entity, "Application not found.");

        entity.Title = request.Title ?? entity.Title;
        entity.Description = request.Description ?? entity.Description;
        entity.Status = request.Status;

        await _context.SaveChangesAsync(cancellationToken);

    }
}
