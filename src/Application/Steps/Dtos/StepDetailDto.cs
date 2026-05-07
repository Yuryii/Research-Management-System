using RMS.Domain.Entities.Models;

namespace RMS.Application.Steps.Dtos;

public record StepDetailDto
{
    public required Guid Id { get; init; }
    public required Guid StepId { get; init; }
    public required string Name { get; init; }
    public int Order { get; init; }
    public Guid? NextStepDetailId { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<StepDetail, StepDetailDto>();
        }
    }
}
