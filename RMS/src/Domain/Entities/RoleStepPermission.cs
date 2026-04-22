using RMS.Domain.Entities.Models;

namespace RMS.Domain.Entities;

public class RoleStepPermission
{
    public Guid RoleId { get; set; }
    public Guid StepId { get; set; }
    public Step Step { get; set; } = null!;
}
