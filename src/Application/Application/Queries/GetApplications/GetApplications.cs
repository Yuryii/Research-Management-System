using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using RMS.Application.Application.Dtos;
using RMS.Application.Common.Interfaces;
using RMS.Application.Common.Models;
using RMS.Domain.Entities.Models;

namespace RMS.Application.Application.Queries.GetApplications;

public record GetApplicationsQuery : IRequest<PaginatedResult<ApplicationDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public class GetApplicationsQueryValidator : AbstractValidator<GetApplicationsQuery>
{
    public GetApplicationsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100);
    }
}

public class GetApplicationsQueryHandler : IRequestHandler<GetApplicationsQuery, PaginatedResult<ApplicationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetApplicationsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<ApplicationDto>> Handle(GetApplicationsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Applications
            .AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.Id)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .AsNoTracking()
            .ProjectTo<ApplicationDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<ApplicationDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}
