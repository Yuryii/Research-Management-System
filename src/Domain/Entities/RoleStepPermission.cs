using RMS.Domain.Entities.Models;

namespace RMS.Domain.Entities;

public class RoleStepPermission
{
    public string RoleId { get; set; } = string.Empty;
    public Guid StepId { get; set; }
    public Step Step { get; set; } = null!;
}
