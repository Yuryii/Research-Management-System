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
            StepDetailId = firstStepDetailId
        };

        _context.Applications.Add(application);

        // Add ApplicationFiles and Save Files
        IReadOnlyList<string> savedFilePaths = [];
        if (request.Files.Count > 0)
        {
            var folders = $"{Config.Store.ROOT_PATH}/{Config.Store.APPLICATION_PATH}";
            savedFilePaths = await _fileService.SaveFilesAsync(
                request.Files,
                Config.Store.AllowedMimeTypes,
                folders,
                cancellationToken);

            for (var index = 0; index < request.Files.Count; index++)
            {
                var file = request.Files[index];
                var savedFilePath = savedFilePaths[index];

                _context.ApplicationFiles.Add(new ApplicationFile
                {
                    ApplicationId = application.Id,
                    File = new RMS.Domain.Entities.Models.File
                    {
                        Name = file.FileName,
                        ContentType = file.ContentType,
                        Length = file.Length,
                        Path = savedFilePath
                    },
                    StepId = stepId
                });
            }
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            foreach (var filePath in savedFilePaths)
            {
                _fileService.DeleteFile(filePath, cancellationToken);
            }

            throw;
        }

        return application.Id;
    }
}
