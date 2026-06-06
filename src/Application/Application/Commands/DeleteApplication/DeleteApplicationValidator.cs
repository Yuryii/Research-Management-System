using System;
using System.Collections.Generic;
using System.Text;
using RMS.Application.Common.Interfaces;

namespace RMS.Application.Application.Commands.DeleteApplication;

public class DeleteApplicationCommandValidator : AbstractValidator<DeleteApplicationCommand>
{
    private readonly IApplicationDbContext _applicationDbContext;
    public DeleteApplicationCommandValidator(IApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;

        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Application Id is required.")
            .MustAsync(async (id, ct) => await IsDraftStatus(id, ct))
            .WithMessage("Only applications in Draft status can be deleted.");
    }

    private async Task<bool> IsDraftStatus(Guid id, CancellationToken cancellationToken)
    {
        var application = await _applicationDbContext.Applications.FindAsync(new object[] { id }, cancellationToken);
        return application is not null && application.Status == Domain.Enums.ApplicationStatus.Draft;
    }
}
