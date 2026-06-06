using RMS.Application.Common.Security;
using RMS.Domain.Constants;

namespace RMS.Application.Application.Commands.ReturnApplication;

[Authorize(Roles = $"{Roles.Administrator}, {Roles.Tttv}, {Roles.Dvqltt}, {Roles.KhcnHtqt}")]
public record ReturnApplicationCommand : IRequest<Guid>
{
    public Guid ApplicationId { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public IFormFileCollection Files { get; init; } = null!;
    public string? RecipientId { get; init; }
}
