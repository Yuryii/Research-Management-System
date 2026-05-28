using RMS.Application.Application.Dtos;
using RMS.Application.Common.Models;
using RMS.Domain.Enums;

namespace RMS.Application.Application.Queries.GetApplications;

public record GetApplicationsQuery : IRequest<PaginatedResult<ApplicationDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public Guid? StepId { get; set; }
    public ApplicationStatus? Status { get; set; }
    public string? Search { get; set; }
}
