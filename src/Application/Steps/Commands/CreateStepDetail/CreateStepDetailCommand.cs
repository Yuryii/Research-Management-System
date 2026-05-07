using RMS.Application.Common.Security;
using RMS.Domain.Constants;

namespace RMS.Application.Steps.Commands.CreateStepDetail;

[Authorize(Roles = Roles.Administrator)]
public record CreateStepDetailCommand : IRequest<Guid>
{
    public required Guid StepId { get; init; }
    public required string Name { get; init; }
    public int Order { get; init; }
    public Guid? NextStepDetailId { get; init; }
    public bool IsReturnStep { get; set; }
}
