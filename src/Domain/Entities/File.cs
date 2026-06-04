namespace RMS.Domain.Entities.Models;

public partial class File : BaseAuditableEntity<Guid>
{
    public required string Name { get; set; }

    public required string Path { get; set; }

    public required string ContentType { get; set; }

    public required long Length { get; set; }

    public virtual IList<ApplicationFile> ApplicationFiles { get; set; } = new List<ApplicationFile>();
    public virtual IList<ApplicationReturnFile> ApplicationReturnFiles { get; set; } = new List<ApplicationReturnFile>();
}
