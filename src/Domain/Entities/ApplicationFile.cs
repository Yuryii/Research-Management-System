using System;

namespace RMS.Domain.Entities.Models;

public partial class ApplicationFile
{
    public Guid ApplicationId { get; set; }
    public Guid FileId { get; set; }
    public Application Application { get; set; } = null!;
    public File File { get; set; } = null!;
    public Guid StepId { get; set; }
    public Step Step { get; set; } = null!;
}
