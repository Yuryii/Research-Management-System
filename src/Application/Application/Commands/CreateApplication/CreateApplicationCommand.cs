using System;
using System.Collections.Generic;
using System.Text;
using RMS.Application.Common.Models;
using RMS.Domain.Enums;

namespace RMS.Application.Application.Commands.CreateApplication;

public record CreateApplicationCommand : IRequest<Guid>
{
    public required string Title { get; init; }
    public required string Description { get; init; }
    public ApplicationStatus Status { get; set; }
    public IReadOnlyList<FileUploadDto> Files { get; init; } = [];
}
