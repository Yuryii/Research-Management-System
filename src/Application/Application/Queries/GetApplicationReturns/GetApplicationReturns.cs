using Microsoft.EntityFrameworkCore;
using RMS.Application.Application.Dtos;
using RMS.Application.Common.Extensions;
using RMS.Application.Common.Interfaces;
using RMS.Application.Common.Models;
using RMS.Domain.Entities;

namespace RMS.Application.Application.Queries.GetApplicationReturns;

public class GetApplicationReturnsQueryHandler : IRequestHandler<GetApplicationReturnsQuery, PaginatedResult<ApplicationReturnDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;

    public GetApplicationReturnsQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        IUser user,
        IIdentityService identityService)
    {
        _context = context;
        _mapper = mapper;
        _user = user;
        _identityService = identityService;
    }

    public async Task<PaginatedResult<ApplicationReturnDto>> Handle(GetApplicationReturnsQuery request, CancellationToken cancellationToken)
    {
        IQueryable<ApplicationReturn> query = _context.ApplicationReturns
            .Include(x => x.Application)
                .ThenInclude(a => a.ApplicationFiles)
                    .ThenInclude(af => af.File)
            .Include(x => x.ApplicationReturnFiles)
                .ThenInclude(arf => arf.File)
            .Where(x => x.CreatedBy == _user.Id || x.Application.CreatedBy == _user.Id);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchLower = request.Search.Trim().ToLower();
            query = query.Where(x =>
                x.Title.ToLower().Contains(searchLower) ||
                x.Description.ToLower().Contains(searchLower) ||
                x.Application.Code.ToLower().Contains(searchLower) ||
                x.Application.Title.ToLower().Contains(searchLower));
        }

        var result = await query.ToPaginatedResultAsync<ApplicationReturn, ApplicationReturnDto, Guid>(
            request, x => x.Id, _mapper.ConfigurationProvider, cancellationToken);

        var returnIds = result.Items.Select(x => x.Id).ToList();
        var filesByReturnId = await _context.ApplicationReturnFiles
            .Where(arf => returnIds.Contains(arf.ApplicationReturnId))
            .Include(arf => arf.File)
            .GroupBy(arf => arf.ApplicationReturnId)
            .ToDictionaryAsync(g => g.Key, g => g.Select(arf => new FileDto
            {
                Id = arf.File.Id,
                Name = arf.File.Name,
                Path = arf.File.Path,
                ContentType = arf.File.ContentType,
                Length = arf.File.Length
            }).ToList(), cancellationToken);

        foreach (var item in result.Items)
        {
            if (!string.IsNullOrEmpty(item.CreatedBy))
            {
                item.CreatorName = await _identityService.GetFullNameAsync(item.CreatedBy);
            }

            if (!string.IsNullOrEmpty(item.RecipientId))
            {
                item.RecipientName = await _identityService.GetFullNameAsync(item.RecipientId);
            }

            if (filesByReturnId.TryGetValue(item.Id, out var files))
            {
                item.Files = files;
            }
        }

        return result;
    }
}
