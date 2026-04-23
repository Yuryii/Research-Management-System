using System;
using System.Collections.Generic;

namespace RMS.Domain.Entities.Models;

public partial class ApplicationFile
{
    public Guid ApplicationId { get; set; }
    public Guid FileId { get; set; }
    public  Application Application { get; set; } = null!;
    public  File File { get; set; } = null!;
}
