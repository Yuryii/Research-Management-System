using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using RMS.Application.Common.Security;
using RMS.Domain.Constants;
using RMS.Domain.Enums;

namespace RMS.Application.Application.Commands.CreateApplication;

[Authorize(Roles = $"{Roles.Teacher},{Roles.Administrator}")]
public class CreateApplicationCommand : IRequest<Guid>
{
    public required string Title { get; init; }
    public required string Description { get; init; }
    public ApplicationStatus Status { get; set; }
    public IFormFileCollection Files { get; set; } = null!;
}
