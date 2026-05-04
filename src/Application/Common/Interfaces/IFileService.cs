using RMS.Application.Common.Models;
using DomainFile = RMS.Domain.Entities.Models.File;
namespace RMS.Application.Common.Interfaces;

public interface IFileService
{
    Task<DomainFile> SaveFileAsync(FileUploadDto file, CancellationToken cancellationToken = default, string? subFolder = null);
    Task<IReadOnlyList<DomainFile>> SaveFilesAsync(IReadOnlyList<FileUploadDto> files, CancellationToken cancellationToken = default, string? subFolder = null);
    Task<bool> DeleteFileAsync(Guid fileId, CancellationToken cancellationToken = default);
}
