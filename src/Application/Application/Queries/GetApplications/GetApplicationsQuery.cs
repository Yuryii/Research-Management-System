using RMS.Application.Application.Dtos;
using RMS.Application.Common.Models;
using RMS.Domain.Enums;

namespace RMS.Application.Application.Queries.GetApplications;

public record GetApplicationsQuery : PagedQuery<ApplicationDto>
{
    public Guid? StepDetailId { get; set; }
    public Guid? StepId { get; set; }
    public ApplicationStatus? Status { get; set; }
    public string? Search { get; set; }
}
