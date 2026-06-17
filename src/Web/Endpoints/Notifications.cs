using Microsoft.AspNetCore.Http.HttpResults;
using RMS.Application.Common.Models;
using RMS.Application.Notifications.Commands.MarkAllNotificationsAsRead;
using RMS.Application.Notifications.Commands.MarkNotificationAsRead;
using RMS.Application.Notifications.Dtos;
using RMS.Application.Notifications.Queries.GetMyNotifications;
using RMS.Application.Notifications.Queries.GetUnreadNotificationCount;

namespace RMS.Web.Endpoints;

public class Notifications : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(GetMyNotifications, "");
        groupBuilder.MapGet(GetUnreadCount, "unread-count");
        groupBuilder.MapPatch(MarkAsRead, "{id:guid}/read");
        groupBuilder.MapPost(MarkAllAsRead, "read-all");
    }

    [EndpointSummary("Get current user's notifications")]
    [EndpointDescription("Retrieves paginated notifications for the authenticated user, newest first.")]
    public static async Task<Ok<PaginatedResult<NotificationDto>>> GetMyNotifications(
        ISender sender, int pageNumber = 1, int pageSize = 10)
    {
        var result = await sender.Send(new GetMyNotificationsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
        });

        return TypedResults.Ok(result);
    }

    [EndpointSummary("Get unread notification count")]
    [EndpointDescription("Returns the number of unread notifications for the authenticated user.")]
    public static async Task<Ok<int>> GetUnreadCount(ISender sender)
    {
        var count = await sender.Send(new GetUnreadNotificationCountQuery());

        return TypedResults.Ok(count);
    }

    [EndpointSummary("Mark a notification as read")]
    [EndpointDescription("Marks the specified notification as read. Only the recipient can mark it.")]
    public static async Task<NoContent> MarkAsRead(ISender sender, Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new MarkNotificationAsReadCommand { Id = id }, cancellationToken);

        return TypedResults.NoContent();
    }

    [EndpointSummary("Mark all notifications as read")]
    [EndpointDescription("Marks all of the authenticated user's unread notifications as read.")]
    public static async Task<Ok<int>> MarkAllAsRead(ISender sender, CancellationToken cancellationToken)
    {
        var updated = await sender.Send(new MarkAllNotificationsAsReadCommand(), cancellationToken);

        return TypedResults.Ok(updated);
    }
}
