using RMS.Domain.Entities.Models;

namespace RMS.Domain.Entities;

public class RoleStepPermission
{
    public string RoleId { get; set; } = string.Empty;
    public Guid StepDetailId { get; set; }
    public StepDetail StepDetail { get; set; } = null!;
}
