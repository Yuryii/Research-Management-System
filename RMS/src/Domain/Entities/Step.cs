namespace RMS.Domain.Entities.Models;

public partial class Step : BaseAuditableEntity<Guid>
{

    public required string Name { get; set; }
    public virtual IList<StepDetail> StepDetails { get; set; } = new List<StepDetail>();
}
