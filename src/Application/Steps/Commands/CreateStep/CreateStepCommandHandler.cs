using RMS.Application.Common.Interfaces;
using RMS.Domain.Entities.Models;

namespace RMS.Application.Steps.Commands.CreateStep;

public class CreateStepCommandHandler : IRequestHandler<CreateStepCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateStepCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateStepCommand request, CancellationToken cancellationToken)
    {
        var entity = new Step
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            ShortName = request.ShortName,
            Order = request.Order
        };

        _context.Steps.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
