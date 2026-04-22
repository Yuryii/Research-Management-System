namespace RMS.Domain.Entities.Models;

public partial class StepDetail : BaseAuditableEntity<Guid>
{

    public required string NameUserScreen { get; set; }

    public required string NameAdminScreen { get; set; }

    public Guid StepId { get; set; }

    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();

    public virtual Step Step { get; set; } = null!;
}
