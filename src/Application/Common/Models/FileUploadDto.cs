namespace RMS.Application.Common.Models;

public record FileUploadDto
{
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public required long Length { get; init; }
    public required Stream Stream { get; init; }
}
