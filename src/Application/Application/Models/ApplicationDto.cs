using RMS.Domain.Entities.Models;
using RMS.Domain.Enums;
using DomainApplication = RMS.Domain.Entities.Models.Application;

namespace RMS.Application.Application.Dtos;

public record ApplicationDto
{
    public required Guid Id { get; init; }
    public required string Code { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required ApplicationStatus Status { get; init; }
    public required Guid StepDetailId { get; init; }
    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<DomainApplication, ApplicationDto>();
        }
    }
}
