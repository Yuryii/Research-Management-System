using RMS.Domain.Entities.Models;
using RMS.Domain.Enums;
using DomainApplication = RMS.Domain.Entities.Models.Application;
using DomainFile = RMS.Domain.Entities.Models.File;

namespace RMS.Application.Application.Dtos;

public record ApplicationDto
{
    public required Guid Id { get; init; }
    public required string Code { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required ApplicationStatus Status { get; init; }
    public required Guid StepDetailId { get; init; }
    public List<ApplicationFileDto> MyApplications { get; set; } = new List<ApplicationFileDto>();
    public List<ApplicationFileDto> PreAttachments { get; set; } = new List<ApplicationFileDto>();

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<DomainApplication, ApplicationDto>();
            CreateMap<ApplicationFile, ApplicationFileDto>();
            CreateMap<DomainFile, FileDto>();
        }
    }
}

public record ApplicationFileDto
{
    public required Guid ApplicationId { get; init; }
    public required Guid FileId { get; init; }
    public required FileDto File { get; init; }
}

public record FileDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Path { get; init; }
    public required string ContentType { get; init; }
    public required long Length { get; init; }
}
