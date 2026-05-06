namespace RMS.Domain.Entities.Models;

public partial class Step : BaseAuditableEntity<Guid>
{

    public required string Name { get; set; }
    public string ShortName { get; set; } = string.Empty;
    public int Order { get; set; }
    public IList<StepDetail> StepDetails { get; set; } = new List<StepDetail>();
    public IList<RoleStepPermission> RoleStepPermissions { get; set; } = new List<RoleStepPermission>();
    public IList<ApplicationFile> ApplicationFiles { get; set; } = new List<ApplicationFile>();

}
