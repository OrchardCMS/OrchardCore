using System.Security.Claims;

namespace OrchardCore.Security.Services;

public interface IRoleService
{
    Task<IEnumerable<IRole>> GetRolesAsync();

    Task<IEnumerable<Claim>> GetRoleClaimsAsync(string role, CancellationToken cancellationToken = default);

    Task<IEnumerable<string>> GetNormalizedRoleNamesAsync();

    ValueTask<bool> IsAdminRoleAsync(string role)
        => ValueTask.FromResult(false);

    ValueTask<bool> IsSystemRoleAsync(string role)
        => ValueTask.FromResult(false);
}
