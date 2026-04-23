using Azure.Core;
using RMS.Application.Common.Models;

namespace RMS.Web.Services;

public class File
{
    public static List<FileUploadDto> FilesToFileDtos (IFormFileCollection? files)
    {
        var fileDtos = new List<FileUploadDto>();
        if (files is not null)
        {
            foreach (var formFile in files.Where(f => f.Length > 0))
            {
                var stream = formFile.OpenReadStream();
                fileDtos.Add(new FileUploadDto
                {
                    FileName = formFile.FileName,
                    ContentType = formFile.ContentType,
                    Length = formFile.Length,
                    Stream = stream
                });
            }
        }
        return fileDtos;
    }
}
