using RMS.Application.Application.Dtos;
using RMS.Application.Common.Models;

namespace RMS.Application.Application.Queries.GetApplicationReturns;

public record GetApplicationReturnsQuery : PagedQuery<ApplicationReturnDto>
{
    public string? Search { get; set; }
}
