using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using RMS.Application.Application.Commands.ReturnApplication;
using RMS.Application.Common.Interfaces;

namespace RMS.Infrastructure.DomainServices;

public class StepResolver : IStepResolver
{

    private readonly IApplicationDbContext _context;
    public StepResolver(IApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<Guid> ResolveAsync(CancellationToken cancellationToken)
    {
        var stepDetailId = await _context.StepDetails
            .OrderBy(x => x.Step.Order)
            .ThenBy(x => x.Order)
            .Select(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return stepDetailId == Guid.Empty
            ? throw new InvalidOperationException(
                "No StepDetail found. Please ensure the workflow is seeded.")
            : stepDetailId;
    }
}
