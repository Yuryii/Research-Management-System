namespace RMS.Domain.Entities.Models;

public partial class AcademicDegree : BaseAuditableEntity<Guid>
{
    public required string Title { get; set; }

    public required string Description { get; set; }
}
