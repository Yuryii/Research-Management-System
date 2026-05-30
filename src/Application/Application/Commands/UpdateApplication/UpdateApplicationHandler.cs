using FluentValidation.Results;
using RMS.Application.Application.Commands.ForwardNextToStep;
using RMS.Application.Common.Interfaces;
using RMS.Domain.Constants;
using RMS.Domain.Entities.Models;
using RMS.Domain.Enums;
using DomainApplication = RMS.Domain.Entities.Models.Application;
using AppValidationException = RMS.Application.Common.Exceptions.ValidationException;

namespace RMS.Application.Application.Commands.UpdateApplication;

public class UpdateApplicationCommandHandler : IRequestHandler<UpdateApplicationCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISender _sender;

    public UpdateApplicationCommandHandler(IApplicationDbContext context, ISender sender)
    {
        _context = context;
        _sender = sender;
    }

    public async Task Handle(UpdateApplicationCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Applications
            .Include(a => a.ApplicationFiles)
                .ThenInclude(af => af.File)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        Guard.Against.NotFound(request.Id, entity, "Application not found.");

        if (entity.Status != ApplicationStatus.Draft)
        {
            throw new AppValidationException(new[] { new ValidationFailure("Status", "Only applications in Draft status can be updated.") });
        }

        if (request.Title is not null)
            entity.Title = request.Title;

        if (request.Description is not null)
            entity.Description = request.Description;

        if (request.Status.HasValue)
            entity.Status = request.Status.Value;

        if (request.Status == ApplicationStatus.Submitted)
        {
            await _sender.Send(
                new ForwardNextToStepCommand { ApplicationId = entity.Id },
                cancellationToken);
            return;
        }

        if (request.FileIds is not null)
        {
            var filesToRemove = entity.ApplicationFiles
                .Where(af => !request.FileIds.Contains(af.FileId))
                .ToList();

            foreach (var appFile in filesToRemove)
            {
                System.IO.File.Delete(appFile.File.Path);
            }
            _context.ApplicationFiles.RemoveRange(filesToRemove);
            _context.Files.RemoveRange(filesToRemove.Select(af => af.File));
        }
        await _context.SaveChangesAsync(cancellationToken);
    }
}
