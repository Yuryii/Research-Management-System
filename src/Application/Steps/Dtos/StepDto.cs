using RMS.Domain.Entities.Models;

namespace RMS.Application.Steps.Dtos;

public record StepDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string ShortName { get; init; } = string.Empty;
    public int Order { get; init; }
    public IList<StepDetailDto> StepDetails { get; init; } = new List<StepDetailDto>();

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Step, StepDto>();
        }
    }
}
