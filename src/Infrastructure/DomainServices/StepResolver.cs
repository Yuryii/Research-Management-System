using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
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
        var defaultStep = await _context.StepDetails
            .OrderBy(sd => sd.StepId)
            .ThenBy(sd => sd.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return defaultStep?.Id
            ?? throw new InvalidOperationException("No StepDetail found. Please ensure the workflow is seeded.");
    }
}
