using RMS.Application.Common.Security;
using Microsoft.AspNetCore.Http;

namespace RMS.Application.Application.Commands.CreateApplicationFiles;

public record CreateApplicationFilesCommand : IRequest
{
    public Guid ApplicationId { get; init; }
    public IFormFileCollection Files { get; init; } = null!;
}
