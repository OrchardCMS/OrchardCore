using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Security;

namespace OrchardCore.Users.Services;

public class NullRoleStore : IRoleClaimStore<IRole>, IQueryableRoleStore<IRole>
{
    public IQueryable<IRole> Roles => Enumerable.Empty<IRole>().AsQueryable();

    public Task AddClaimAsync(IRole role, Claim claim, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task<IdentityResult> CreateAsync(IRole role, CancellationToken cancellationToken)
        => Task.FromResult(IdentityResult.Success);

    public Task<IdentityResult> DeleteAsync(IRole role, CancellationToken cancellationToken)
        => Task.FromResult(IdentityResult.Success);

#pragma warning disable CA1816
    public void Dispose()
    {
    }
#pragma warning restore CA1816

    public Task<IRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        => Task.FromResult<IRole>(null);

    public Task<IRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        => Task.FromResult<IRole>(null);

    public Task<IList<Claim>> GetClaimsAsync(IRole role, CancellationToken cancellationToken = default)
        => Task.FromResult<IList<Claim>>(new List<Claim>());

    public Task<string> GetNormalizedRoleNameAsync(IRole role, CancellationToken cancellationToken)
        => Task.FromResult<string>(null);

    public Task<string> GetRoleIdAsync(IRole role, CancellationToken cancellationToken)
        => Task.FromResult<string>(null);

    public Task<string> GetRoleNameAsync(IRole role, CancellationToken cancellationToken)
        => Task.FromResult<string>(null);

    public Task RemoveClaimAsync(IRole role, Claim claim, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task SetNormalizedRoleNameAsync(IRole role, string normalizedName, CancellationToken cancellationToken)
        => Task.CompletedTask;

    public Task SetRoleNameAsync(IRole role, string roleName, CancellationToken cancellationToken)
        => Task.CompletedTask;

    public Task<IdentityResult> UpdateAsync(IRole role, CancellationToken cancellationToken)
        => Task.FromResult(IdentityResult.Success);
}
