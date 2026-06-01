using Microsoft.EntityFrameworkCore;
using RMS.Application.Application.Dtos;
using RMS.Application.Common.Exceptions;
using RMS.Application.Common.Interfaces;

namespace RMS.Infrastructure.DomainServices;

/// <summary>
/// Provides query-related operations for applications, including step context resolution
/// and attachment retrieval across workflow steps.
/// </summary>
public class ApplicationQueryService : IApplicationQueryService
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    /// <summary>
    /// Initialises a new instance of <see cref="ApplicationQueryService"/>.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="identityService">The identity service for role lookups.</param>
    public ApplicationQueryService(IApplicationDbContext context, IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    /// <summary>
    /// Resolves the current step context for a user, including the active step detail,
    /// the associated workflow step, the previous step (if any), and all file attachments
    /// attached to both the current and previous steps.
    /// </summary>
    /// <remarks>
    /// When <paramref name="explicitStepDetailId"/> is not provided, the step detail is
    /// derived from the user's roles by looking up the lowest-ordered step each role is
    /// permitted to access. Attachments for the previous step are only loaded when the
    /// current step is not the first step in the workflow (i.e. <c>Order > 1</c>).
    /// </remarks>
    /// <param name="roles">The collection of role names assigned to the current user.</param>
    /// <param name="explicitStepDetailId">
    /// An optional explicit step detail identifier. When provided, role-based resolution
    /// is skipped and this value is used directly.
    /// </param>
    /// <param name="cancellationToken">A token used to observe cancellation requests.</param>
    /// <returns>An <see cref="ApplicationQueryContext"/> containing the resolved step and attachment data.</returns>
    /// <exception cref="ForbiddenAccessException">
    /// Thrown when the user has no roles assigned, or when no step detail is available
    /// for any of the user's roles.
    /// </exception>
    public async Task<ApplicationQueryContext> ResolveStepContextAsync(
        IReadOnlyList<string> roles,
        Guid? explicitStepDetailId,
        CancellationToken cancellationToken)
    {
        var stepDetailId = explicitStepDetailId;

        if (!stepDetailId.HasValue)
        {
            if (roles is null || roles.Count == 0)
            {
                throw new ForbiddenAccessException("User does not have any roles assigned.");
            }

            var roleIds = await _identityService.GetRoleIdsAsync(roles, cancellationToken);

            stepDetailId = await _context.RoleStepPermissions
                .Where(x => roleIds.Contains(x.RoleId))
                .OrderBy(x => x.Step.Order)
                .SelectMany(x => x.Step.StepDetails.OrderBy(sd => sd.Order))
                .Select(sd => sd.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (stepDetailId == Guid.Empty)
            {
                throw new ForbiddenAccessException("No step detail is available for the current user roles.");
            }
        }

        var currentStepDetail = await _context.StepDetails
            .Where(sd => sd.Id == stepDetailId)
            .Select(sd => new { sd.StepId, Step = sd.Step })
            .FirstOrDefaultAsync(cancellationToken);

        var currentStepId = currentStepDetail?.StepId ?? Guid.Empty;

        var currentStepAttachments = await _context.ApplicationFiles
            .Where(x => x.StepId == currentStepId)
            .Select(x => new
            {
                x.ApplicationId,
                File = new FileDto
                {
                    Id = x.File.Id,
                    Name = x.File.Name,
                    Path = x.File.Path,
                    ContentType = x.File.ContentType,
                    Length = x.File.Length
                }
            })
            .ToListAsync(cancellationToken);

        var currentStepAttachmentsByApplication = currentStepAttachments
            .GroupBy(x => x.ApplicationId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.File).ToList());

        Dictionary<Guid, List<FileDto>> previousStepAttachmentsByApplication = new();

        if (currentStepDetail is not null && currentStepDetail.Step.Order > 0)
        {
            var preStep = await _context.Steps
                .Where(x => x.Order == currentStepDetail.Step.Order - 1)
                .FirstOrDefaultAsync(cancellationToken);

            if (preStep is not null)
            {
                var preStepAttachments = await _context.ApplicationFiles
                    .Where(x => x.StepId == preStep.Id)
                    .Select(x => new
                    {
                        x.ApplicationId,
                        File = new FileDto
                        {
                            Id = x.File.Id,
                            Name = x.File.Name,
                            Path = x.File.Path,
                            ContentType = x.File.ContentType,
                            Length = x.File.Length
                        }
                    })
                    .ToListAsync(cancellationToken);

                previousStepAttachmentsByApplication = preStepAttachments
                    .GroupBy(x => x.ApplicationId)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.File).ToList());
            }
        }

        return new ApplicationQueryContext(
            stepDetailId!.Value,
            currentStepId,
            currentStepDetail?.Step.Order > 0 ? currentStepId : null,
            currentStepAttachmentsByApplication,
            previousStepAttachmentsByApplication
        );
    }
}
