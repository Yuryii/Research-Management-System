using RMS.Domain.Entities.Models;

namespace RMS.Application.Steps.Queries.GetStepAndStepDetail;

public class StepDto
{
    public required string Name { get; set; }
    public string ShortName { get; set; } = string.Empty;
    public int Order { get; set; }
    public IList<StepDetailDto> StepDetails { get; set; } = new List<StepDetailDto>();

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Step, StepDto>()
                .ForMember(d => d.StepDetails, opt => opt.MapFrom(s => s.StepDetails));
            CreateMap<StepDetail, StepDetailDto>();
        }
    }
}

public class StepDetailDto
{
    public required string Name { get; set; }
    public int Order { get; set; }
}
