using RMS.Application.Common.Interfaces;
using RMS.Application.Common.Models;
using RMS.Domain.Constants;
using DomainFile = RMS.Domain.Entities.Models.File;

namespace RMS.Infrastructure.Services;

public class LocalFileService : IFileService
{
    private readonly IApplicationDbContext _context;
    private readonly string _uploadRootPath;

    public LocalFileService(IApplicationDbContext context)
    {
        _context = context;
        _uploadRootPath = Path.Combine(Directory.GetCurrentDirectory(), Config.Store.ROOT_PATH);

        if (!Directory.Exists(_uploadRootPath))
            Directory.CreateDirectory(_uploadRootPath);
    }

    public async Task<DomainFile> SaveFileAsync(FileUploadDto file, CancellationToken cancellationToken = default, string? subFolder = null)
    {
        var folder = string.IsNullOrWhiteSpace(subFolder) ? string.Empty : subFolder.Trim();
        var targetFolderPath = string.IsNullOrEmpty(folder)
            ? _uploadRootPath
            : Path.Combine(_uploadRootPath, folder);

        if (!Directory.Exists(targetFolderPath))
            Directory.CreateDirectory(targetFolderPath);

        var id = Guid.NewGuid();
        var extension = Path.GetExtension(file.FileName);
        var storedFileName = $"{id}{extension}";
        var filePath = Path.Combine(targetFolderPath, storedFileName);

        await using var output = File.Create(filePath);
        await file.Stream.CopyToAsync(output, cancellationToken);

        var relativePath = string.IsNullOrEmpty(folder)
            ? storedFileName
            : Path.Combine(folder, storedFileName).Replace('\\', '/');

        var fileEntity = new DomainFile
        {
            Id = id,
            Name = file.FileName,
            Path = relativePath,
            ContentType = file.ContentType,
            Length = file.Length
        };

        _context.Files.Add(fileEntity);
        await _context.SaveChangesAsync(cancellationToken);

        return fileEntity;
    }

    public async Task<IReadOnlyList<DomainFile>> SaveFilesAsync(IReadOnlyList<FileUploadDto> files, CancellationToken cancellationToken = default, string? subFolder = null)
    {
        var savedFiles = new List<DomainFile>(files.Count);

        foreach (var file in files)
        {
            var saved = await SaveFileAsync(file, cancellationToken, subFolder);
            savedFiles.Add(saved);
        }

        return savedFiles;
    }

    public async Task<bool> DeleteFileAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        var fileEntity = await _context.Files.FindAsync([fileId], cancellationToken);
        if (fileEntity is null)
            return false;

        var normalizedPath = fileEntity.Path.Replace('/', Path.DirectorySeparatorChar);
        var filePath = Path.Combine(_uploadRootPath, normalizedPath);

        if (File.Exists(filePath))
            File.Delete(filePath);

        _context.Files.Remove(fileEntity);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
