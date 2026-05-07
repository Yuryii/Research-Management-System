using CleanArchitecture.Application.Common.Interfaces;

namespace CleanArchitecture.Application.ReturnApplication.Queries.Application;

public record ApplicationQuery : IRequest<Guid>
{
}

public class ApplicationQueryValidator : AbstractValidator<ApplicationQuery>
{
    public ApplicationQueryValidator()
    {
    }
}

public class ApplicationQueryHandler : IRequestHandler<ApplicationQuery, Guid>
{
    private readonly IApplicationDbContext _context;

    public ApplicationQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(ApplicationQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
