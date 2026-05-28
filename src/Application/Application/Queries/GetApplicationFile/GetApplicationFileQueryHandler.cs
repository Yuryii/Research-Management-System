using Microsoft.EntityFrameworkCore;
using RMS.Application.Common.Exceptions;
using RMS.Application.Common.Interfaces;

namespace RMS.Application.Application.Queries.GetApplicationFile;

public class GetApplicationFileQueryHandler : IRequestHandler<GetApplicationFileQuery, FileDownloadResult?>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileService _fileService;

    public GetApplicationFileQueryHandler(IApplicationDbContext context, IFileService fileService)
    {
        _context = context;
        _fileService = fileService;
    }

    public async Task<FileDownloadResult?> Handle(GetApplicationFileQuery request, CancellationToken cancellationToken)
    {
        var applicationFile = await _context.ApplicationFiles
            .Include(af => af.File)
            .AsNoTracking()
            .FirstOrDefaultAsync(
                af => af.ApplicationId == request.ApplicationId && af.FileId == request.FileId,
                cancellationToken);

        if (applicationFile?.File == null)
            return null;

        var stream = await _fileService.GetFileAsync(applicationFile.File.Path, cancellationToken);

        return new FileDownloadResult(
            stream,
            applicationFile.File.ContentType,
            applicationFile.File.Name,
            applicationFile.File.Length);
    }
}
