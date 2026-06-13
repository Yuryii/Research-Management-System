using RMS.Domain.Common;
using RMS.Domain.Entities.Models;

namespace RMS.Domain.Entities;

public class ApplicationReturn : BaseAuditableEntity<Guid>
{
    public new Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public Application Application { get; set; } = null!;
    public required string Title { get; set; }
    public required string Description { get; set; }
    public string? RecipientId { get; set; }
    public IList<ApplicationReturnFile> ApplicationReturnFiles { get; set; } = new List<ApplicationReturnFile>();
}
