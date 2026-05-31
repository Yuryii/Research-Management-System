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
    public required string StepDetailName { get; set; }
    public string? CreatedBy { get; init; }
    public List<FileDto> MyApplications { get; set; } = new List<FileDto>();
    public List<FileDto> PreAttachments { get; set; } = new List<FileDto>();
    public string? TeacherName { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<DomainApplication, ApplicationDto>()
                .ForMember(dest => dest.StepDetailName, opt => opt.MapFrom(src => src.StepDetail.Name))
                .ForMember(dest => dest.TeacherName, opt => opt.Ignore())
                .ForMember(dest => dest.MyApplications, opt => opt.Ignore())
                .ForMember(dest => dest.PreAttachments, opt => opt.Ignore());
            CreateMap<DomainFile, FileDto>();
        }
    }
}

public record FileDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Path { get; init; }
    public required string ContentType { get; init; }
    public required long Length { get; init; }
}
