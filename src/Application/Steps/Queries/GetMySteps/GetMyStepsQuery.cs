using RMS.Application.Common.Interfaces;
using RMS.Application.Steps.Dtos;

namespace RMS.Application.Steps.Queries.GetMySteps;

public record GetMyStepsQuery : IRequest<IList<StepDto>>;
