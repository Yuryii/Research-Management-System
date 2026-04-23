using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using RMS.Application.Application.Dtos;
using RMS.Application.Common.Interfaces;

namespace RMS.Application.Application.Queries.GetApplications;

public record GetApplicationsQuery : IRequest<IReadOnlyList<ApplicationDto>>
{
}

public class GetApplicationsQueryValidator : AbstractValidator<GetApplicationsQuery>
{
    public GetApplicationsQueryValidator()
    {
    }
}

public class GetApplicationsQueryHandler : IRequestHandler<GetApplicationsQuery, IReadOnlyList<ApplicationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetApplicationsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ApplicationDto>> Handle(GetApplicationsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Applications
            .AsNoTracking()
            .ProjectTo<ApplicationDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
