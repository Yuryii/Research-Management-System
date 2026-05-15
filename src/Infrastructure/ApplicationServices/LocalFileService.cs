using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using MimeDetective;
using MimeDetective.Definitions;
using RMS.Application.Common.Interfaces;
using RMS.Domain.Constants;
using DomainFile = RMS.Domain.Entities.Models.File;

namespace RMS.Infrastructure.Services;

public class LocalFileService : IFileService
{
    private readonly IApplicationDbContext _context;

    public LocalFileService(IApplicationDbContext context)
    {
        _context = context;
    }


    public async Task<string> SaveFileAsync(
        IFormFile file,
        HashSet<string> allowedMimeTypes,
        string subFolder,
        CancellationToken cancellationToken = default)
    {
        ValidateFile(file, allowedMimeTypes);

        var folder = CreateDirectory(subFolder);

        var filePath = GenFilePath(folder, file);

        await using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);

        await file.CopyToAsync(stream, cancellationToken);

        return NormalizeStoredPath(filePath);
    }

    public async Task<IReadOnlyList<string>> SaveFilesAsync(
        IReadOnlyList<IFormFile> files,
        HashSet<string> allowedMimeTypes,
        string subFolder,
        CancellationToken cancellationToken = default)
    {
        List<string> filePaths = new List<string>();
        foreach (var file in files)
        {
            var filePath = await SaveFileAsync(file, allowedMimeTypes, subFolder, cancellationToken);
            filePaths.Add(filePath);
        }

        return filePaths;
    }

    public void DeleteFile(string path, CancellationToken cancellationToken = default)
    {
        string fullPath = Path.GetFullPath(path);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }

    public Stream GetFile(string path)
    {
        string fullPath = Path.GetFullPath(path);

        if (!File.Exists(fullPath))
            throw new FileNotFoundException();

        return new FileStream(
            fullPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            4096,
            useAsync: true);
    }
    // Helpers
    private string CreateDirectory(string? folderPath)
    {
        var folder = string.IsNullOrWhiteSpace(folderPath)
            ? Directory.GetCurrentDirectory()
            : folderPath.Trim();

        var fullTargetPath = Path.GetFullPath(folder);
        Directory.CreateDirectory(fullTargetPath);

        return fullTargetPath;
    }
    private string GenFilePath(string folderPath, IFormFile file)
    {
        var id = Guid.NewGuid();
        var extension = Path.GetExtension(file.FileName);
        var storedFileName = $"{id}{extension}";
        var filePath = Path.Combine(folderPath, storedFileName);

        return filePath;
    }

    private string NormalizeStoredPath(string filePath)
    {
        var fullPath = Path.GetFullPath(filePath);
        return fullPath.Replace('\\', '/');
    }

    private void ValidateFile(IFormFile file, HashSet<string> allowedMimeTypes)
    {
        if (file == null || file.Length == 0)
        {
            throw new ValidationException("File không hợp lệ");
        }

        var inspector = new ContentInspectorBuilder
        {
            Definitions = MimeDetective.Definitions.DefaultDefinitions.All()
        }.Build();

        using var stream = file.OpenReadStream();

        var result = inspector.Inspect(stream).FirstOrDefault();

        if (result == null)
        {
            throw new ValidationException("Không xác định được loại file");
        }

        string mimeType = result.Definition.File.MimeType
            ?? throw new ValidationException("Không xác định được loại file");

        if (!allowedMimeTypes.Contains(mimeType))
        {
            throw new ValidationException(
                $"File type không được hỗ trợ: {mimeType}");
        }
    }
}
