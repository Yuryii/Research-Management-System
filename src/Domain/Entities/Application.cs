namespace RMS.Domain.Entities.Models;

public partial class Application : BaseAuditableEntity<Guid>
{
    public required string Code { get; set; }

    public required string Title { get; set; }

    public required string Description { get; set; }

    public ApplicationStatus Status { get; set; }

    public Guid StepDetailId { get; set; }
    public  IList<ApplicationFile> ApplicationFiles { get; set; } = new List<ApplicationFile>();

    public  StepDetail StepDetail { get; set; } = null!;
}
