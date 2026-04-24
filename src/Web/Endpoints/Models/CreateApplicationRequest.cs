using RMS.Domain.Enums;

namespace RMS.Web.Endpoints.Models;

public record CreateApplicationRequest
{
    public required string Title { get; init; }
    public required string Description { get; init; }
    public ApplicationStatus Status { get; set; }
    public IFormFileCollection? Files { get; init; }
}
