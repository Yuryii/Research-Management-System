using RMS.Application.Common.Security;

namespace RMS.Application.Application.Commands.UpdateApplicationStepDetail;

[Authorize]
public record UpdateApplicationStepDetailCommand : IRequest
{
    public Guid ApplicationId { get; init; }

    public Guid StepDetailId { get; init; }
}
