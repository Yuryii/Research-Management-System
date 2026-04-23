using RMS.Application.Common.Models;

namespace RMS.Application.Common.Interfaces;

public interface IFileService
{
    Task<RMS.Domain.Entities.Models.File> SaveFileAsync(FileUploadDto file, CancellationToken cancellationToken = default, string? subFolder = null);
    Task<IReadOnlyList<RMS.Domain.Entities.Models.File>> SaveFilesAsync(IReadOnlyList<FileUploadDto> files, CancellationToken cancellationToken = default, string? subFolder = null);
}
