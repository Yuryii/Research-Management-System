namespace RMS.Domain.Entities.Models;

public partial class Step : BaseAuditableEntity<Guid>
{

    public required string Name { get; set; }
    public IList<StepDetail> StepDetails { get; set; } = new List<StepDetail>();
    public IList<RoleStepPermission> RoleStepPermission { get; set; } = new List<RoleStepPermission>();

}
