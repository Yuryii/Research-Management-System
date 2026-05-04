using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using RMS.Application.Common.Interfaces;
using RMS.Application.Common.Models;
using DomainFile = RMS.Domain.Entities.Models.File;

namespace RMS.Infrastructure.Services;

public sealed class S3StorageOptions
{
    public const string SectionName = "Storage:S3";

    public string BucketName { get; set; } = string.Empty;

    public string Prefix { get; set; } = "uploads";
}

public class S3FileService : IFileService
{
    private readonly IApplicationDbContext _context;
    private readonly IAmazonS3 _s3Client;
    private readonly S3StorageOptions _options;

    public S3FileService(IApplicationDbContext context, IAmazonS3 s3Client, IOptions<S3StorageOptions> options)
    {
        _context = context;
        _s3Client = s3Client;
        _options = options.Value;
    }

    public async Task<DomainFile> SaveFileAsync(FileUploadDto file, CancellationToken cancellationToken = default, string? subFolder = null)
    {
        Guard.Against.NullOrWhiteSpace(_options.BucketName, message: "S3 bucket name is required when Storage:Provider is S3.");

        var id = Guid.NewGuid();
        var extension = Path.GetExtension(file.FileName);
        var storedFileName = $"{id}{extension}";
        var objectKey = BuildObjectKey(storedFileName, subFolder);

        var putRequest = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = objectKey,
            InputStream = file.Stream,
            ContentType = file.ContentType,
            AutoCloseStream = false
        };

        putRequest.Metadata["original-file-name"] = file.FileName;

        await _s3Client.PutObjectAsync(putRequest, cancellationToken);

        var fileEntity = new DomainFile
        {
            Id = id,
            Name = file.FileName,
            Path = objectKey,
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
        Guard.Against.NullOrWhiteSpace(_options.BucketName, message: "S3 bucket name is required when Storage:Provider is S3.");

        var fileEntity = await _context.Files.FindAsync([fileId], cancellationToken);
        if (fileEntity is null)
            return false;

        var deleteRequest = new DeleteObjectRequest
        {
            BucketName = _options.BucketName,
            Key = fileEntity.Path
        };

        await _s3Client.DeleteObjectAsync(deleteRequest, cancellationToken);

        _context.Files.Remove(fileEntity);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    private string BuildObjectKey(string storedFileName, string? subFolder)
    {
        var segments = new List<string>();

        AddPathSegments(segments, _options.Prefix);
        AddPathSegments(segments, subFolder);
        segments.Add(storedFileName);

        return string.Join('/', segments);
    }

    private static void AddPathSegments(ICollection<string> segments, string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        foreach (var segment in path.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            segments.Add(segment);
        }
    }
}
