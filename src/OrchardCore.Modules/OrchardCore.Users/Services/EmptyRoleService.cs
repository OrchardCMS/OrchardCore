using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using OrchardCore.Security;
using OrchardCore.Security.Services;

namespace OrchardCore.Users.Services;

public class EmptyRoleService : IRoleService
{
    public Task<IEnumerable<string>> GetNormalizedRoleNamesAsync()
    {
        return Task.FromResult(Enumerable.Empty<string>());
    }

    public Task<IEnumerable<Claim>> GetRoleClaimsAsync(string role, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Enumerable.Empty<Claim>());
    }

    public Task<IEnumerable<IRole>> GetRolesAsync()
    {
        return Task.FromResult(Enumerable.Empty<IRole>());
    }
}
