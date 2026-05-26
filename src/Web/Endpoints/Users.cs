using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RMS.Infrastructure.Identity;

namespace RMS.Web.Endpoints;

public class Users : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapIdentityApi<ApplicationUser>();

        groupBuilder.MapGet(GetInfoWithRoles, "manage/info")
            .WithOrder(-1)
            .RequireAuthorization();

        groupBuilder.MapPost(Logout, "logout").RequireAuthorization();
    }

    [EndpointSummary("Get account info")]
    [EndpointDescription("Returns the current user's email, confirmation status, and roles.")]
    public static async Task<Results<Ok<InfoWithRolesResponse>, UnauthorizedHttpResult>> GetInfoWithRoles(
        UserManager<ApplicationUser> userManager,
        ClaimsPrincipal principal)
    {
        var user = await userManager.GetUserAsync(principal);

        if (user == null)
        {
            return TypedResults.Unauthorized();
        }

        var roles = await userManager.GetRolesAsync(user);

        return TypedResults.Ok(new InfoWithRolesResponse(
            user.Email ?? string.Empty,
            user.EmailConfirmed,
            roles.ToArray()));
    }

    [EndpointSummary("Log out")]
    [EndpointDescription("Logs out the current user by clearing the authentication cookie.")]
    public static async Task<Results<Ok, UnauthorizedHttpResult>> Logout(SignInManager<ApplicationUser> signInManager, [FromBody] object empty)
    {
        if (empty != null)
        {
            await signInManager.SignOutAsync();
            return TypedResults.Ok();
        }

        return TypedResults.Unauthorized();
    }

    public record InfoWithRolesResponse(string Email, bool IsEmailConfirmed, string[] Roles);
}
