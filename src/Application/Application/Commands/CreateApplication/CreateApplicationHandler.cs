using RMS.Application.Application.Commands.CreateApplication;
using RMS.Application.Application.Commands.ForwardNextToStep;
using RMS.Application.Common.Interfaces;
using RMS.Domain.Entities.Models;
using RMS.Domain.Enums;
using RMS.Domain.Interfaces;
using DomainApplication = RMS.Domain.Entities.Models.Application;

namespace RMS.Application.Application.Commands.CreateApplication;
public class CreateApplicationCommandHandler : IRequestHandler<CreateApplicationCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IApplicationFileService _applicationFileService;
    private readonly ICodeGeneratorService _codeGeneratorService;
    private readonly IStepResolver _stepResolver;
    private readonly ISender _sender;
    private readonly IUser _user;

    public CreateApplicationCommandHandler(
        IApplicationDbContext context,
        IApplicationFileService applicationFileService,
        ICodeGeneratorService codeGeneratorService,
        IStepResolver stepResolver,
        ISender sender,
        IUser user)
    {
        _context = context;
        _applicationFileService = applicationFileService;
        _codeGeneratorService = codeGeneratorService;
        _stepResolver = stepResolver;
        _sender = sender;
        _user = user;
    }

    public async Task<Guid> Handle(CreateApplicationCommand request, CancellationToken cancellationToken)
    {
        var firstStepDetailId = await _stepResolver.ResolveAsync(cancellationToken);

        var stepId = await _context.StepDetails
            .Where(x => x.Id == firstStepDetailId)
            .Select(x => x.StepId)
            .SingleAsync(cancellationToken);

        var code = _codeGeneratorService.GenerateApplicationCode(request.Title);

        var application = new DomainApplication
        {
            Id = Guid.NewGuid(),
            Code = code,
            Title = request.Title,
            Description = request.Description,
            Status = request.Status,
            StepDetailId = firstStepDetailId,
            CreatedBy = _user.Id
        };

        _context.Applications.Add(application);

        if (request.Files.Count > 0)
        {
            await _applicationFileService.AddFilesToApplicationAsync(
                application.Id,
                stepId,
                request.Files,
                cancellationToken);
        }
        else
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        if (request.Status == ApplicationStatus.Submitted)
        {
            await _sender.Send(
                new ForwardNextToStepCommand { ApplicationId = application.Id },
                cancellationToken);
        }

        return application.Id;
    }
}
