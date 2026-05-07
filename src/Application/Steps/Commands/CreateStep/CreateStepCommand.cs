using RMS.Application.Common.Security;
using RMS.Domain.Constants;

namespace RMS.Application.Steps.Commands.CreateStep;

[Authorize(Roles = Roles.Administrator)]
public record CreateStepCommand : IRequest<Guid>
{
    public required string Name { get; init; }
    public string ShortName { get; init; } = string.Empty;
    public int Order { get; init; }
}
