using RMS.Application.Common.Interfaces;
using RMS.Domain.Constants;
using RMS.Domain.Entities;
using RMS.Domain.Entities.Models;

namespace RMS.Application.Application.Commands.ReturnApplication;

public class ReturnApplicationCommandHandler : IRequestHandler<ReturnApplicationCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileService _fileService;

    public ReturnApplicationCommandHandler(IApplicationDbContext context, IFileService fileService)
    {
        _context = context;
        _fileService = fileService;
    }

    public async Task<Guid> Handle(ReturnApplicationCommand request, CancellationToken cancellationToken)
    {
        var application = await _context.Applications
            .SingleOrDefaultAsync(a => a.Id == request.ApplicationId, cancellationToken);

        Guard.Against.NotFound(request.ApplicationId, application, "Application not found.");

        var returnStepDetail = await _context.StepDetails
            .AsNoTracking()
            .SingleOrDefaultAsync(sd => sd.IsReturnStep, cancellationToken);

        Guard.Against.NotFound(request.ApplicationId, returnStepDetail, "Return step detail not found.");

        application.StepDetailId = returnStepDetail.Id;

        var applicationReturn = new ApplicationReturn
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            RecipientId = request.RecipientId ?? application.CreatedBy
        };

        _context.ApplicationReturns.Add(applicationReturn);

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

                _context.ApplicationReturnFiles.Add(new ApplicationReturnFile
                {
                    ApplicationReturnId = applicationReturn.Id,
                    File = new RMS.Domain.Entities.Models.File
                    {
                        Name = file.FileName,
                        ContentType = file.ContentType,
                        Length = file.Length,
                        Path = savedFilePath
                    }
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

        return applicationReturn.Id;
    }
}
