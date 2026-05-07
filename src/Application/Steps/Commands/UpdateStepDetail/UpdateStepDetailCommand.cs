using RMS.Application.Common.Security;
using RMS.Domain.Constants;

namespace RMS.Application.Steps.Commands.UpdateStepDetail;

[Authorize(Roles = Roles.Administrator)]
public record UpdateStepDetailCommand : IRequest
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public int? Order { get; init; }
    public Guid? NextStepDetailId { get; init; }
    public Guid IsReturnStep { get; set; }
}
