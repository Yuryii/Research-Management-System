using Microsoft.AspNetCore.Http.HttpResults;
using RMS.Application.Steps.Commands.CreateStep;
using RMS.Application.Steps.Commands.CreateStepDetail;
using RMS.Application.Steps.Commands.DeleteStep;
using RMS.Application.Steps.Commands.DeleteStepDetail;
using RMS.Application.Steps.Commands.UpdateStep;
using RMS.Application.Steps.Commands.UpdateStepDetail;
using RMS.Application.Steps.Queries.GetStepAndStepDetail;

namespace RMS.Web.Endpoints;

public class Steps : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(GetStepAndStepDetail, "{stepDetailId:guid}");
        groupBuilder.MapPost(CreateStep);
        groupBuilder.MapPut(UpdateStep, "{id:guid}");
        groupBuilder.MapDelete(DeleteStep, "{id:guid}");

        groupBuilder.MapPost(CreateStepDetail, "StepDetails");
        groupBuilder.MapPut(UpdateStepDetail, "StepDetails/{id:guid}");
        groupBuilder.MapDelete(DeleteStepDetail, "StepDetails/{id:guid}");
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

    [EndpointSummary("Create a Step")]
    [EndpointDescription("Creates a new step and returns the created step ID.")]
    public static async Task<Created<Guid>> CreateStep(ISender sender, CreateStepCommand command)
    {
        var id = await sender.Send(command);

        return TypedResults.Created($"/{nameof(Steps)}/{id}", id);
    }

    [EndpointSummary("Update a Step")]
    [EndpointDescription("Updates the specified step. The ID in the URL must match the ID in the payload.")]
    public static async Task<Results<NoContent, BadRequest>> UpdateStep(ISender sender, Guid id, UpdateStepCommand command)
    {
        if (id != command.Id) return TypedResults.BadRequest();

        await sender.Send(command);

        return TypedResults.NoContent();
    }

    [EndpointSummary("Delete a Step")]
    [EndpointDescription("Deletes the step with the specified ID.")]
    public static async Task<NoContent> DeleteStep(ISender sender, Guid id)
    {
        await sender.Send(new DeleteStepCommand(id));

        return TypedResults.NoContent();
    }

    [EndpointSummary("Create a Step Detail")]
    [EndpointDescription("Creates a new step detail and returns the created step detail ID.")]
    public static async Task<Created<Guid>> CreateStepDetail(ISender sender, CreateStepDetailCommand command)
    {
        var id = await sender.Send(command);

        return TypedResults.Created($"/{nameof(Steps)}/StepDetails/{id}", id);
    }

    [EndpointSummary("Update a Step Detail")]
    [EndpointDescription("Updates the specified step detail. The ID in the URL must match the ID in the payload.")]
    public static async Task<Results<NoContent, BadRequest>> UpdateStepDetail(ISender sender, Guid id, UpdateStepDetailCommand command)
    {
        if (id != command.Id) return TypedResults.BadRequest();

        await sender.Send(command);

        return TypedResults.NoContent();
    }

    [EndpointSummary("Delete a Step Detail")]
    [EndpointDescription("Deletes the step detail with the specified ID.")]
    public static async Task<NoContent> DeleteStepDetail(ISender sender, Guid id)
    {
        await sender.Send(new DeleteStepDetailCommand(id));

        return TypedResults.NoContent();
    }
}
