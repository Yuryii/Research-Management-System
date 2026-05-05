using System;
using System.Collections.Generic;
using System.Text;
using RMS.Application.Common.Security;
using RMS.Domain.Constants;
using RMS.Domain.Entities.Models;
using RMS.Domain.Enums;

namespace RMS.Application.Application.Commands.UpdateApplication;

[Authorize]
public record UpdateApplicationCommand : IRequest
{
    public Guid Id { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public ApplicationStatus Status { get; set; }
}
