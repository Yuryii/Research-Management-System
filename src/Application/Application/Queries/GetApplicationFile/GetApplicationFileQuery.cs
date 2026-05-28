using RMS.Application.Common.Interfaces;
using RMS.Domain.Entities.Models;

namespace RMS.Application.Application.Queries.GetApplicationFile;

public record GetApplicationFileQuery(Guid ApplicationId, Guid FileId) : IRequest<FileDownloadResult?>;

public record FileDownloadResult(
    Stream Stream,
    string ContentType,
    string FileName,
    long FileLength);
