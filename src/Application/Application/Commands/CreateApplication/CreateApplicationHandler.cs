using RMS.Application.Application.Commands.CreateApplication;
using RMS.Application.Common.Interfaces;
using RMS.Domain.Constants;
using RMS.Domain.Entities.Models;
using RMS.Domain.Enums;
using RMS.Domain.Interfaces;
using DomainApplication = RMS.Domain.Entities.Models.Application;

namespace RMS.Application.Application.Commands.CreatApplication;
public class CreateApplicationCommandHandler : IRequestHandler<CreateApplicationCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileService _fileService;
    private readonly ICodeGeneratorService _codeGeneratorService;
    private readonly IStepResolver _stepResolver;

    public CreateApplicationCommandHandler(IApplicationDbContext context, IFileService fileService, ICodeGeneratorService codeGeneratorService, IStepResolver stepResolver)
    {
        _context = context;
        _fileService = fileService;
        _codeGeneratorService = codeGeneratorService;
        _stepResolver = stepResolver;
    }

    public async Task<Guid> Handle(CreateApplicationCommand request, CancellationToken cancellationToken)
    {
        // Add Application
        var stepDetailId = request.StepDetailId ?? await _stepResolver.ResolveAsync(cancellationToken);
        var code = _codeGeneratorService.GenerateApplicationCode(request.Title);

        var application = new DomainApplication
        {
            Id = Guid.NewGuid(),
            Code = code,
            Title = request.Title,
            Description = request.Description,
            Status = ApplicationStatus.Draft,
            StepDetailId = stepDetailId
        };

        _context.Applications.Add(application);

        // Add ApplicationFiles and Save Files
        if (request.Files.Count > 0)
        {
            var savedFiles = await _fileService.SaveFilesAsync(request.Files, cancellationToken, Config.Store.APPLICATION_PATH);
            foreach (var file in savedFiles)
            {
                _context.ApplicationFiles.Add(new ApplicationFile
                {
                    ApplicationId = application.Id,
                    FileId = file.Id
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return application.Id;
    }
}
