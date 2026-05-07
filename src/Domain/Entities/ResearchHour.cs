using System;
using System.Collections.Generic;

namespace RMS.Domain.Entities.Models;

public partial class ResearchHour : BaseAuditableEntity<Guid>
{
    public required string Hours { get; set; }
    public DateTime CreatedAt { get; set; }
}
