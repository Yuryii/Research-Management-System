using RMS.Application.Common.Interfaces;

namespace RMS.Application.Application.Commands.DeleteApplication;

public record DeleteApplicationCommand : IRequest<Guid>
{
}

public class DeleteApplicationCommandValidator : AbstractValidator<DeleteApplicationCommand>
{
    public DeleteApplicationCommandValidator()
    {
    }
}

public class DeleteApplicationCommandHandler : IRequestHandler<DeleteApplicationCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public DeleteApplicationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(DeleteApplicationCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
