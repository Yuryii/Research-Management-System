using Microsoft.AspNetCore.Http;
using RMS.Application.Common.Interfaces;
using RMS.Domain.Constants;
using RMS.Domain.Entities.Models;
using DomainFile = RMS.Domain.Entities.Models.File;

namespace RMS.Infrastructure.Services;

public class ApplicationFileService : IApplicationFileService
{
    private readonly IApplicationDbContext _context;
    private readonly IFileService _fileService;

    public ApplicationFileService(IApplicationDbContext context, IFileService fileService)
    {
        _context = context;
        _fileService = fileService;
    }

    public async Task AddFilesToApplicationAsync(
        Guid applicationId,
        Guid stepId,
        IFormFileCollection files,
        CancellationToken cancellationToken = default)
    {
        var folder = $"{Config.Store.ROOT_PATH}/{Config.Store.APPLICATION_PATH}";

        var savedFilePaths = await _fileService.SaveFilesAsync(
            files.ToList(),
            Config.Store.AllowedMimeTypes,
            folder,
            cancellationToken);

        for (var index = 0; index < files.Count; index++)
        {
            var file = files[index];
            var savedFilePath = savedFilePaths[index];

            _context.ApplicationFiles.Add(new ApplicationFile
            {
                ApplicationId = applicationId,
                File = new DomainFile
                {
                    Name = file.FileName,
                    ContentType = file.ContentType,
                    Length = file.Length,
                    Path = savedFilePath
                },
                StepId = stepId
            });
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
    }
}
