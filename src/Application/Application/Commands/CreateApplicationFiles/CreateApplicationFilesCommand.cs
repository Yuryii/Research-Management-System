using RMS.Application.Common.Security;
using Microsoft.AspNetCore.Http;

namespace RMS.Application.Application.Commands.CreateApplicationFiles;

public record CreateApplicationFilesCommand : IRequest
{
    public Guid ApplicationId { get; init; }
    public List<IFormFile> Files { get; init; } = [];
}
