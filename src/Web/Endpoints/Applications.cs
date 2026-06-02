using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RMS.Application.Application.Commands.CreateApplication;
using RMS.Application.Application.Commands.DeleteApplication;
using RMS.Application.Application.Commands.ForwardNextToStep;
using RMS.Application.Application.Commands.ReturnApplication;
using RMS.Application.Application.Commands.UpdateApplication;
using RMS.Application.Application.Commands.UpdateApplicationStepDetail;
using RMS.Application.Application.Dtos;
using RMS.Application.Application.Queries.GetApplications;
using RMS.Application.Common.Models;
using RMS.Domain.Entities.Models;
using RMS.Domain.Enums;

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
        groupBuilder.MapPost(ReturnApplication, "ReturnApplication").DisableAntiforgery();
    }

    [EndpointSummary("Get all Applications")]
    [EndpointDescription("Retrieves applications with pagination.")]
    public static async Task<Results<Ok<PaginatedResult<ApplicationDto>>, BadRequest<string>>> GetApplications(ISender sender, int pageNumber = 1, int pageSize = 10, Guid? stepDetailId = null, Guid? stepId = null, ApplicationStatus? status = null, string? search = null)
    {
        var applications = await sender.Send(new GetApplicationsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            StepDetailId = stepDetailId,
            StepId = stepId,
            Status = status,
            Search = search
        });

        return TypedResults.Ok(applications);
    }

    [EndpointSummary("Create a new Application")]
    [EndpointDescription("Creates a new application with optional file attachments and returns the ID of the created application.")]
    [Consumes("multipart/form-data")]
    public static async Task<Created<Guid>> CreateApplication(
        ISender sender,
        [FromForm] CreateApplicationCommand command,
        CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);

        return TypedResults.Created($"/{nameof(Applications)}/{id}", id);
    }

    [EndpointSummary("Update a Application")]
    [EndpointDescription("Updates the specified application. The ID in the URL must match the ID in the payload.")]
    public static async Task<Results<NoContent, BadRequest>> UpdateApplication(ISender sender, Guid id, [FromBody] UpdateApplicationCommand command)
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

    [EndpointSummary("Return an Application")]
    [EndpointDescription("Returns an application to the return step, saves notification and files.")]
    [Consumes("multipart/form-data")]
    public static async Task<Results<Ok<Guid>, BadRequest<string>>> ReturnApplication(
        ISender sender,
        [FromForm] ReturnApplicationCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var notificationId = await sender.Send(command, cancellationToken);
            return TypedResults.Ok(notificationId);
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    }


}
