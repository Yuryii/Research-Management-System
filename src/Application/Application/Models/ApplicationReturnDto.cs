using RMS.Application.Common.Models;
using RMS.Domain.Entities;
using DomainReturnFile = RMS.Domain.Entities.Models.ApplicationReturnFile;

namespace RMS.Application.Application.Dtos;

public record ApplicationReturnDto
{
    public required Guid Id { get; init; }
    public required Guid ApplicationId { get; init; }
    public required string ApplicationCode { get; init; }
    public required string ApplicationTitle { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public string? RecipientId { get; init; }
    public string? RecipientName { get; set; }
    public required DateTimeOffset Created { get; init; }
    public required string CreatedBy { get; init; }
    public string? CreatorName { get; set; }
    public List<FileDto> Files { get; set; } = new();

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<ApplicationReturn, ApplicationReturnDto>()
                .ForMember(dest => dest.ApplicationCode, opt => opt.MapFrom(src => src.Application.Code))
                .ForMember(dest => dest.ApplicationTitle, opt => opt.MapFrom(src => src.Application.Title))
                .ForMember(dest => dest.RecipientName, opt => opt.Ignore())
                .ForMember(dest => dest.CreatorName, opt => opt.Ignore())
                .ForMember(dest => dest.Files, opt => opt.MapFrom(src => src.ApplicationReturnFiles));
            CreateMap<DomainReturnFile, FileDto>()
                .ConvertUsing(src => new FileDto
                {
                    Id = src.File.Id,
                    Name = src.File.Name,
                    Path = src.File.Path,
                    ContentType = src.File.ContentType,
                    Length = src.File.Length,
                });
        }
    }
}
