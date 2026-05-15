using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RMS.Application.Application.Commands.CreateApplicationFiles;
using RMS.Application.Application.Commands.DeleteApplicationFiles;

namespace RMS.Web.Endpoints;

public class ApplicationFiles : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapPost(Create, "CreateApplicationFiles").DisableAntiforgery();
        groupBuilder.MapDelete(Delete, "{applicationId:guid}/{fileId:guid}");
    }

    [EndpointSummary("Create application files")]
[EndpointDescription("Uploads files for an application and associates them with its current step.")]
    public static async Task<NoContent> Create(
        ISender sender,
        [FromForm] CreateApplicationFilesCommand command,
        CancellationToken cancellationToken)
    {
        await sender.Send(command, cancellationToken);

        return TypedResults.NoContent();
    }

    [EndpointSummary("Delete application file")]
    [EndpointDescription("Deletes an application file and removes the underlying stored file.")]
    public static async Task<NoContent> Delete(ISender sender, Guid applicationId, Guid fileId)
    {
        await sender.Send(new DeleteApplicationFilesCommand
        {
            ApplicationId = applicationId,
            FileId = fileId
        });

        return TypedResults.NoContent();
    }
}
