using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RMS.Application.Application.Commands.CreateApplication;
using RMS.Web.Endpoints.Models;
using ApplicationFile = RMS.Web.Services.File;

namespace RMS.Web.Endpoints;

public class Applications : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapPost(CreateApplication, "CreateApplication").DisableAntiforgery();
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
            StepDetailId = request.StepDetailId,
            Files = ApplicationFile.FilesToFileDtos(request.Files)
        };

        var id = await sender.Send(command, cancellationToken);

        return TypedResults.Created($"/{nameof(Applications)}/{id}", id);
    }
}
