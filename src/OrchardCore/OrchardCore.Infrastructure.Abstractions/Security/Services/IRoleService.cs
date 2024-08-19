using System.Security.Claims;

namespace OrchardCore.Security.Services;

public interface IRoleService
{
    Task<IEnumerable<IRole>> GetRolesAsync();
    Task<IEnumerable<Claim>> GetRoleClaimsAsync(string role, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetNormalizedRoleNamesAsync();
}
