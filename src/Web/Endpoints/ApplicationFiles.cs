using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RMS.Application.Application.Commands.CreateApplicationFiles;
using RMS.Application.Application.Commands.DeleteApplicationFiles;
using RMS.Application.Application.Queries.GetApplicationFile;

namespace RMS.Web.Endpoints;

public class ApplicationFiles : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapPost(Create, "CreateApplicationFiles").DisableAntiforgery();
        groupBuilder.MapDelete(Delete, "{applicationId:guid}/{fileId:guid}");
        groupBuilder.MapGet(Download, "{applicationId:guid}/{fileId:guid}")
            .WithName("DownloadApplicationFile")
            .WithSummary("Download an application file")
            .WithDescription("Streams a file attached to an application.");
    }

    [EndpointSummary("Create application files")]
    [EndpointDescription("Uploads files for an application and associates them with its current step.")]
    [Consumes("multipart/form-data")]
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

    [BinaryContent]
    [EndpointSummary("Download an application file")]
    [EndpointDescription("Streams the file content back to the client.")]
    public static async Task<Results<FileStreamHttpResult, NotFound>> Download(
        ISender sender,
        Guid applicationId,
        Guid fileId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetApplicationFileQuery(applicationId, fileId), cancellationToken);

        if (result == null)
            return TypedResults.NotFound();

        return TypedResults.File(
            result.Stream,
            result.ContentType,
            result.FileName,
            enableRangeProcessing: true);
    }
}
