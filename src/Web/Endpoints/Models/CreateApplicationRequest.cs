namespace RMS.Web.Endpoints.Models;

public record CreateApplicationRequest
{
    public required string Title { get; init; }
    public required string Description { get; init; }
    public Guid? StepDetailId { get; init; }
    public IFormFileCollection? Files { get; init; }
}
