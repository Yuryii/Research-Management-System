using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RMS.Application.Application.Commands.CreateApplication;
using RMS.Application.Application.Commands.UpdateApplication;
using RMS.Application.TodoLists.Commands.UpdateTodoList;
using RMS.Web.Endpoints.Models;
using ApplicationFile = RMS.Web.Services.File;

namespace RMS.Web.Endpoints;

public class Applications : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapPost(CreateApplication, "CreateApplication").DisableAntiforgery();
        groupBuilder.MapPost(UpdateApplication, "UpdateApplication");
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
}
