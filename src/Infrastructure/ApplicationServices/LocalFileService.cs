using System.ComponentModel.DataAnnotations;
using System.IO.Compression;
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
            throw new FileNotFoundException($"File not found: {path}");

        return new FileStream(
            fullPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            4096,
            useAsync: true);
    }

    public Task<Stream> GetFileAsync(string path, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(GetFile(path));
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
        var currentDir = Directory.GetCurrentDirectory();
        return Path.GetRelativePath(currentDir, fullPath).Replace('\\', '/');
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

        string detectedMime = result.Definition.File.MimeType
            ?? throw new ValidationException("Không xác định được loại file");

        // Trường hợp đặc biệt: file ZIP-based (bao gồm docx, xlsx, pptx, zip, jar, apk, v.v.)
        if (detectedMime == "application/x-compressed" ||
            detectedMime == "application/zip" ||
            detectedMime == "application/x-zip-compressed")
        {
            // BẮT BUỘC kiểm tra nội dung bên trong
            ValidateZipBasedFile(file, allowedMimeTypes);
            return;
        }


        if (!allowedMimeTypes.Contains(detectedMime))
        {
            throw new ValidationException(
                $"File type không được hỗ trợ: {detectedMime}");
        }
    }
    private void ValidateZipBasedFile(IFormFile file, HashSet<string> allowedMimeTypes)
    {
        using var stream = file.OpenReadStream();
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: false);

        // 1. Kiểm tra có phải Office Open XML không
        bool isOfficeFile = archive.Entries.Any(e =>
            e.FullName.Equals("[Content_Types].xml", StringComparison.OrdinalIgnoreCase));

        if (isOfficeFile)
        {
            // Xác định loại Office file cụ thể
            string realMime = DetermineOfficeMimeType(archive, file.FileName);

            if (!allowedMimeTypes.Contains(realMime))
            {
                throw new ValidationException($"File type không được hỗ trợ: {realMime}");
            }

            // KIỂM TRA THÊM: không chứa macro độc hại trong .docm, .xlsm
            if (HasMacroOrScript(archive))
            {
                throw new ValidationException("File chứa macro/script không an toàn");
            }

            return;
        }

        // 2. Nếu không phải Office file -> kiểm tra xem có được phép upload ZIP không
        if (!allowedMimeTypes.Contains("application/zip"))
        {
            throw new ValidationException("Không cho phép upload file nén");
        }

        // 3. Nếu cho phép ZIP -> kiểm tra nội dung bên trong
        foreach (var entry in archive.Entries)
        {
            // Không cho phép file thực thi
            string ext = Path.GetExtension(entry.Name).ToLower();
            var dangerousExtensions = new[] { ".exe", ".dll", ".bat", ".cmd", ".ps1", ".sh", ".js", ".vbs", ".jar", ".class" };

            if (dangerousExtensions.Contains(ext))
            {
                throw new ValidationException($"File nén chứa file độc hại: {entry.Name}");
            }

            // Kiểm tra đường dẫn (tránh directory traversal: ../../config.php)
            if (entry.FullName.Contains("..") || Path.IsPathRooted(entry.FullName))
            {
                throw new ValidationException("File nén chứa đường dẫn không an toàn");
            }

            // Giới hạn kích thước giải nén (tránh zip bomb)
            if (entry.Length > 10 * 1024 * 1024) // 10MB mỗi file
            {
                throw new ValidationException($"File {entry.Name} quá lớn sau giải nén");
            }
        }
    }

    private string DetermineOfficeMimeType(ZipArchive archive, string fileName)
    {
        // Có thể kiểm tra file /word/document.xml, /xl/workbook.xml, /ppt/presentation.xml
        if (archive.Entries.Any(e => e.FullName.StartsWith("word/")))
            return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
        if (archive.Entries.Any(e => e.FullName.StartsWith("xl/")))
            return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        if (archive.Entries.Any(e => e.FullName.StartsWith("ppt/")))
            return "application/vnd.openxmlformats-officedocument.presentationml.presentation";

        // Fallback dùng extension
        return Path.GetExtension(fileName).ToLower() switch
        {
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            _ => "application/zip"
        };
    }

    private bool HasMacroOrScript(ZipArchive archive)
    {
        // Kiểm tra file .bin có chứa VBA macro không
        return archive.Entries.Any(e =>
            e.FullName.EndsWith(".bin", StringComparison.OrdinalIgnoreCase) &&
            e.Name.Contains("vba", StringComparison.OrdinalIgnoreCase));
    }
}
