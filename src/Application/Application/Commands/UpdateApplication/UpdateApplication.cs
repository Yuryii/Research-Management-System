using RMS.Application.Common.Interfaces;

namespace RMS.Application.Application.Commands.UpdateApplication;

public record UpdateApplicationCommand : IRequest<Guid>
{
}

public class UpdateApplicationCommandValidator : AbstractValidator<UpdateApplicationCommand>
{
    public UpdateApplicationCommandValidator()
    {
    }
}

public class UpdateApplicationCommandHandler : IRequestHandler<UpdateApplicationCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public UpdateApplicationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(UpdateApplicationCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
