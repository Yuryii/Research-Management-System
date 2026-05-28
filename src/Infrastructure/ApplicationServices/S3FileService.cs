using System.ComponentModel.DataAnnotations;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MimeDetective;
using MimeDetective.Definitions;
using RMS.Application.Common.Interfaces;

namespace RMS.Infrastructure.Services;

public sealed class S3StorageOptions
{
    public const string SectionName = "Storage:S3";

    public string BucketName { get; set; } = string.Empty;

    public string Prefix { get; set; } = "uploads";
}

public class S3FileService : IFileService
{
    private readonly IAmazonS3 _s3Client;
    private readonly S3StorageOptions _options;

    public S3FileService(IAmazonS3 s3Client, IOptions<S3StorageOptions> options)
    {
        _s3Client = s3Client;
        _options = options.Value;
    }

    public async Task<string> SaveFileAsync(
        IFormFile file,
        HashSet<string> allowedMimeTypes,
        string subFolder,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrWhiteSpace(_options.BucketName, message: "S3 bucket name is required when Storage:Provider is S3.");
        ValidateFile(file, allowedMimeTypes);

        var id = Guid.NewGuid();
        var extension = Path.GetExtension(file.FileName);
        var storedFileName = $"{id}{extension}";
        var objectKey = BuildObjectKey(storedFileName, subFolder);

        await using var input = file.OpenReadStream();
        var putRequest = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = objectKey,
            InputStream = input,
            ContentType = file.ContentType,
            AutoCloseStream = false
        };

        putRequest.Metadata["original-file-name"] = file.FileName;

        await _s3Client.PutObjectAsync(putRequest, cancellationToken);

        return objectKey;
    }

    public async Task<IReadOnlyList<string>> SaveFilesAsync(
        IReadOnlyList<IFormFile> files,
        HashSet<string> allowedMimeTypes,
        string subFolder,
        CancellationToken cancellationToken = default)
    {
        var savedFiles = new List<string>(files.Count);

        foreach (var file in files)
        {
            var saved = await SaveFileAsync(file, allowedMimeTypes, subFolder, cancellationToken);
            savedFiles.Add(saved);
        }

        return savedFiles;
    }

    public Stream GetFile(string path)
    {
        throw new NotSupportedException("GetFile is not supported for S3 storage. Use S3 SDK to retrieve objects.");
    }

    public async Task<Stream> GetFileAsync(string path, CancellationToken cancellationToken = default)
    {
        var getRequest = new GetObjectRequest
        {
            BucketName = _options.BucketName,
            Key = path
        };

        var response = await _s3Client.GetObjectAsync(getRequest, cancellationToken);
        return response.ResponseStream;
    }

    public void DeleteFile(string path, CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrWhiteSpace(_options.BucketName, message: "S3 bucket name is required when Storage:Provider is S3.");

        var deleteRequest = new DeleteObjectRequest
        {
            BucketName = _options.BucketName,
            Key = path
        };

        _s3Client.DeleteObjectAsync(deleteRequest, cancellationToken).GetAwaiter().GetResult();

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
