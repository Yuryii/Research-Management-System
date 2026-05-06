namespace RMS.Web.Endpoints.Models;

public record CreateApplicationFilesRequest
{
    public Guid ApplicationId { get; init; }
    public IFormFileCollection? Files { get; init; }
}
