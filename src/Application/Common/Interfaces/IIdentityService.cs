using RMS.Application.Common.Models;

namespace RMS.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(string userId);

    Task<string?> GetFullNameAsync(string userId);

    Task<bool> IsInRoleAsync(string userId, string role);

    Task<IReadOnlyCollection<string>> GetRoleIdsAsync(IEnumerable<string> roleNames, CancellationToken cancellationToken);

    Task<bool> AuthorizeAsync(string userId, string policyName);

    Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password);

    Task<Result> DeleteUserAsync(string userId);
}
