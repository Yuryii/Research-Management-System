namespace RMS.Domain.Entities.Models;

public partial class StepDetail : BaseAuditableEntity<Guid>
{

    public required string Name { get; set; }
    public int Order { get; set; }

    public Guid StepId { get; set; }
    public Guid? NextStepDetailId { get; set; }
    public StepDetail NextStepDetail { get; set; } = null!;

    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();

    public virtual Step Step { get; set; } = null!;
}
