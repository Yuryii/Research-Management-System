using Microsoft.AspNetCore.Http;
using System.IO;
using DomainFile = RMS.Domain.Entities.Models.File;
namespace RMS.Application.Common.Interfaces;

public interface IFileService
{
    Task<string> SaveFileAsync(
        IFormFile file,
        HashSet<string> allowedMimeTypes,
        string subFolder,
        CancellationToken cancellationToken = default
        );
    Task<IReadOnlyList<string>> SaveFilesAsync(
        IReadOnlyList<IFormFile> files,
        HashSet<string> allowedMimeTypes,
        string subFolder,
        CancellationToken cancellationToken = default);
    void DeleteFile(string path, CancellationToken cancellationToken = default);
    Stream GetFile(string path);
    Task<Stream> GetFileAsync(string path, CancellationToken cancellationToken = default);
}
