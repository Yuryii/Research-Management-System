namespace RMS.Domain.Entities.Models;

public partial class ApplicationReturnFile
{
    public Guid ApplicationReturnId { get; set; }
    public ApplicationReturn ApplicationReturn { get; set; } = null!;
    public Guid FileId { get; set; }
    public File File { get; set; } = null!;
}
