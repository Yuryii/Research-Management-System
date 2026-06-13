using Microsoft.AspNetCore.Http.HttpResults;
using RMS.Application.Application.Dtos;
using RMS.Application.Application.Queries.GetApplicationReturns;
using RMS.Application.Common.Models;

namespace RMS.Web.Endpoints;

public class ApplicationReturns : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();
        groupBuilder.MapGet(GetApplicationReturns);
    }

    [EndpointSummary("Get all Application Returns")]
    [EndpointDescription("Retrieves application returns created by the current user with pagination.")]
    public static async Task<Results<Ok<PaginatedResult<ApplicationReturnDto>>, BadRequest<string>>> GetApplicationReturns(
        ISender sender, int pageNumber = 1, int pageSize = 10, string? search = null)
    {
        var result = await sender.Send(new GetApplicationReturnsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Search = search
        });

        return TypedResults.Ok(result);
    }
}
