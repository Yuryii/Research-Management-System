using RMS.Domain.Entities.Models;

namespace RMS.Domain.Entities;

public class RoleStepPermission
{
    public Guid RoleId { get; set; }
    public Guid StepDetailId { get; set; }
    public StepDetail StepDetail { get; set; } = null!;
}
