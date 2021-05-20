using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace OrchardCore.Security.Services
{
    public interface IRoleService
    {
        Task<IEnumerable<IRole>> GetRolesAsync();
        Task<IEnumerable<Claim>> GetRoleClaimsAsync(string role, CancellationToken cancellationToken = default);
        Task<IEnumerable<string>> GetNormalizedRoleNamesAsync();
    }
}
