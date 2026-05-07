using RMS.Application.Common.Security;
using RMS.Domain.Constants;

namespace RMS.Application.Steps.Commands.UpdateStep;

[Authorize(Roles = Roles.Administrator)]
public record UpdateStepCommand : IRequest
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public string? ShortName { get; init; }
    public int? Order { get; init; }
}
