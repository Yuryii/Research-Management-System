using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RMS.Application.Application.Commands.CreateApplication;
using RMS.Application.Application.Commands.DeleteApplication;
using RMS.Application.Application.Commands.ForwardNextToStep;
using RMS.Application.Application.Commands.UpdateApplication;
using RMS.Application.Application.Commands.UpdateApplicationStepDetail;
using RMS.Application.Application.Dtos;
using RMS.Application.Application.Queries.GetApplications;
using RMS.Application.Common.Models;
using RMS.Application.TodoLists.Commands.UpdateTodoList;
using RMS.Domain.Entities.Models;
using RMS.Web.Endpoints.Models;
using ApplicationFile = RMS.Web.Services.File;

namespace RMS.Web.Endpoints;

public class Applications : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(GetApplications);
        groupBuilder.MapPost(CreateApplication, "CreateApplication").DisableAntiforgery();
        groupBuilder.MapPost(UpdateApplication, "UpdateApplication");
        groupBuilder.MapPost(UpdateApplicationStepDetail, "UpdateApplicationStepDetail");
        groupBuilder.MapDelete(DeleteApplication, "{id}");
        groupBuilder.MapPost(ForwardNextToStep, "ForwardNextToStep").DisableAntiforgery();
    }

    [EndpointSummary("Get all Applications")]
    [EndpointDescription("Retrieves applications with pagination.")]
    public static async Task<Results<Ok<PaginatedResult<ApplicationDto>>, BadRequest<string>>> GetApplications(ISender sender, int pageNumber = 1, int pageSize = 10, Guid? stepId = null)
    {
        if(stepId == null)
        {
            return TypedResults.BadRequest("StepId is required.");
        }

        var applications = await sender.Send(new GetApplicationsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            StepId = stepId.Value
        });

        return TypedResults.Ok(applications);
    }

    [EndpointSummary("Create a new Application")]
    [EndpointDescription("Creates a new application with optional file attachments and returns the ID of the created application.")]
    public static async Task<Created<Guid>> CreateApplication(
        ISender sender,
        [FromForm] CreateApplicationRequest request,
        CancellationToken cancellationToken)
    {

        var command = new CreateApplicationCommand
        {
            Title = request.Title,
            Description = request.Description,
            Status = request.Status,
            Files = ApplicationFile.FilesToFileDtos(request.Files)
        };

        var id = await sender.Send(command, cancellationToken);

        return TypedResults.Created($"/{nameof(Applications)}/{id}", id);
    }

    [EndpointSummary("Update a Application")]
    [EndpointDescription("Updates the specified application. The ID in the URL must match the ID in the payload.")]
    public static async Task<Results<NoContent, BadRequest>> UpdateApplication(ISender sender, Guid id, UpdateApplicationCommand command)
    {
        if (id != command.Id) return TypedResults.BadRequest();

        await sender.Send(command);

        return TypedResults.NoContent();
    }

    [EndpointSummary("Update an Application Step Detail")]
    [EndpointDescription("Updates Application.StepDetailId when the current user has a role assigned to the target Step.")]
    public static async Task<NoContent> UpdateApplicationStepDetail(ISender sender, UpdateApplicationStepDetailCommand command)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }

    [EndpointSummary("Delete an Application")]
    [EndpointDescription("Deletes the application with the specified ID.")]
    public static async Task<NoContent> DeleteApplication(ISender sender, Guid id)
    {
        await sender.Send(new DeleteApplicationCommand(id));

        return TypedResults.NoContent();
    }

    [EndpointSummary("Forward an Application to the next Step")]
    [EndpointDescription("Forwards the application to the next step. The user must have a")]
    public static async Task<Results<Ok<Guid>, BadRequest<string>>> ForwardNextToStep(ISender sender, ForwardNextToStepCommand command)
    {
        try
        {
            var applicationId = await sender.Send(command);
            return TypedResults.Ok(applicationId);
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    }


}
