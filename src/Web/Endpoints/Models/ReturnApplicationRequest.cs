namespace RMS.Web.Endpoints.Models;

public record ReturnApplicationRequest
{
    public Guid ApplicationId { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public IFormFileCollection? Files { get; init; }
}
