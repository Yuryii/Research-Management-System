using RMS.Application.Common.Models;
using RMS.Application.Common.Security;

namespace RMS.Application.Application.Commands.CreateApplicationFiles;

[Authorize]
public record CreateApplicationFilesCommand : IRequest<IReadOnlyList<Guid>>
{
    public Guid ApplicationId { get; init; }
    public IReadOnlyList<FileUploadDto> Files { get; init; } = [];
}
