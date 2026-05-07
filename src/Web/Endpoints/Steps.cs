using Microsoft.AspNetCore.Http.HttpResults;
using RMS.Application.Steps.Queries.GetStepAndStepDetail;

namespace RMS.Web.Endpoints;

public class Steps : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(GetStepAndStepDetail, "{stepDetailId:guid}");
    }

    [EndpointSummary("Get step and step details")]
    [EndpointDescription("Retrieves the step and all step details for the specified step detail ID.")]
    public static async Task<Results<Ok<StepDto>, NotFound>> GetStepAndStepDetail(ISender sender, Guid stepDetailId)
    {
        var result = await sender.Send(new GetStepAndStepDetailQuery
        {
            StepDetailId = stepDetailId
        });

        return TypedResults.Ok(result);
    }
}
