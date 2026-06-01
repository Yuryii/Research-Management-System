using RMS.Application.Application.Dtos;

namespace RMS.Application.Common.Interfaces;

public record ApplicationQueryContext(
    Guid StepDetailId,
    Guid CurrentStepId,
    Guid? PreviousStepId,
    Dictionary<Guid, List<FileDto>> CurrentStepAttachments,
    Dictionary<Guid, List<FileDto>> PreviousStepAttachments
);

public interface IApplicationQueryService
{
    Task<ApplicationQueryContext> ResolveStepContextAsync(
        IReadOnlyList<string> roles,
        Guid? explicitStepDetailId,
        CancellationToken cancellationToken);
}
