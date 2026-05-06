using RMS.Application.Common.Security;

namespace RMS.Application.Application.Commands.DeleteApplicationFiles;

[Authorize]
public record DeleteApplicationFilesCommand : IRequest
{
    public Guid ApplicationId { get; init; }
    public Guid FileId { get; init; }
}
