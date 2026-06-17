using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RMS.Application.Common.Interfaces;
using RMS.Application.Common.Models;
using RMS.Infrastructure.Data;

namespace RMS.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IUserClaimsPrincipalFactory<ApplicationUser> _userClaimsPrincipalFactory;
    private readonly IAuthorizationService _authorizationService;
    private readonly ApplicationDbContext _dbContext;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory,
        IAuthorizationService authorizationService,
        ApplicationDbContext dbContext)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        _authorizationService = authorizationService;
        _dbContext = dbContext;
    }

    public async Task<string?> GetUserNameAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user?.UserName;
    }

    public async Task<string?> GetFullNameAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return null;

        var parts = new[] { user.FirstName, user.LastName }
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();

        return parts.Count != 0 ? string.Join(" ", parts) : user.UserName;
    }

    public async Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password)
    {
        var user = new ApplicationUser
        {
            UserName = userName,
            Email = userName,
        };

        var result = await _userManager.CreateAsync(user, password);

        return (result.ToApplicationResult(), user.Id);
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user != null && await _userManager.IsInRoleAsync(user, role);
    }

    public async Task<IReadOnlyCollection<string>> GetRoleIdsAsync(IEnumerable<string> roleNames, CancellationToken cancellationToken)
    {
        var normalizedRoleNames = roleNames
            .Where(roleName => !string.IsNullOrWhiteSpace(roleName))
            .Select(roleName => roleName.ToUpperInvariant())
            .Distinct()
            .ToList();

        if (normalizedRoleNames.Count == 0)
        {
            return [];
        }

        return await _roleManager.Roles
            .Where(role => role.NormalizedName != null && normalizedRoleNames.Contains(role.NormalizedName))
            .Select(role => role.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<string>> GetUserIdsInRolesAsync(IEnumerable<string> roleNames, CancellationToken cancellationToken)
    {
        var normalizedRoleNames = roleNames
            .Where(roleName => !string.IsNullOrWhiteSpace(roleName))
            .Select(roleName => roleName.ToUpperInvariant())
            .Distinct()
            .ToList();

        if (normalizedRoleNames.Count == 0)
        {
            return [];
        }

        var userRoles = _dbContext.Set<IdentityUserRole<string>>();
        var roles = _dbContext.Set<ApplicationRole>();

        var query = from user in _userManager.Users
                    join ur in userRoles on user.Id equals ur.UserId
                    join role in roles on ur.RoleId equals role.Id
                    where role.NormalizedName != null && normalizedRoleNames.Contains(role.NormalizedName)
                    select user.Id;

        return await query.Distinct().ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<string>> GetUserIdsInRoleIdsAsync(IEnumerable<string> roleIds, CancellationToken cancellationToken)
    {
        var ids = roleIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .ToList();

        if (ids.Count == 0)
        {
            return [];
        }

        var userRoles = _dbContext.Set<IdentityUserRole<string>>();

        var query = from user in _userManager.Users
                    join ur in userRoles on user.Id equals ur.UserId
                    where ids.Contains(ur.RoleId)
                    select user.Id;

        return await query.Distinct().ToListAsync(cancellationToken);
    }

    public async Task<bool> AuthorizeAsync(string userId, string policyName)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return false;
        }

        var principal = await _userClaimsPrincipalFactory.CreateAsync(user);

        var result = await _authorizationService.AuthorizeAsync(principal, policyName);

        return result.Succeeded;
    }

    public async Task<Result> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user != null ? await DeleteUserAsync(user) : Result.Success();
    }

    public async Task<Result> DeleteUserAsync(ApplicationUser user)
    {
        var result = await _userManager.DeleteAsync(user);

        return result.ToApplicationResult();
    }
}
