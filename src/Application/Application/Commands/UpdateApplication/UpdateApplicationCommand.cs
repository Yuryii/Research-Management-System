using System;
using System.Collections.Generic;
using System.Text;
using RMS.Domain.Entities.Models;
using RMS.Domain.Enums;

namespace RMS.Application.Application.Commands.UpdateApplication;

public record UpdateApplicationCommand : IRequest
{
    public Guid Id { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public ApplicationStatus Status { get; set; }
}
