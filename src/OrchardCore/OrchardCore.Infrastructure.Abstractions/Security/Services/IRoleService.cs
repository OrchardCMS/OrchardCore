using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace OrchardCore.Security.Services
{
    public interface IRoleService
    {
        Task<IEnumerable<string>> GetRoleNamesAsync();
        Task<IEnumerable<Claim>> GetRoleClaimsAsync(string role, CancellationToken cancellationToken = default);
    }

}
