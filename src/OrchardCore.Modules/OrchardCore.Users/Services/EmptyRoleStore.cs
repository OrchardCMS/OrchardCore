using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Security;

namespace OrchardCore.Users.Services;

public class EmptyRoleStore : IRoleStore<IRole>
{
    public Task<IdentityResult> CreateAsync(IRole role, CancellationToken cancellationToken)
    {
        return Task.FromResult(IdentityResult.Success);
    }

    public Task<IdentityResult> DeleteAsync(IRole role, CancellationToken cancellationToken)
    {
        return Task.FromResult(IdentityResult.Success);
    }

    public void Dispose()
    {
    }

    public Task<IRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
    {
        return Task.FromResult<IRole>(null);
    }

    public Task<IRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
    {
        return Task.FromResult<IRole>(null);
    }

    public Task<string> GetNormalizedRoleNameAsync(IRole role, CancellationToken cancellationToken)
    {
        return Task.FromResult<string>(null);
    }

    public Task<string> GetRoleIdAsync(IRole role, CancellationToken cancellationToken)
    {
        return Task.FromResult<string>(null);
    }

    public Task<string> GetRoleNameAsync(IRole role, CancellationToken cancellationToken)
    {
        return Task.FromResult<string>(null);
    }

    public Task SetNormalizedRoleNameAsync(IRole role, string normalizedName, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task SetRoleNameAsync(IRole role, string roleName, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task<IdentityResult> UpdateAsync(IRole role, CancellationToken cancellationToken)
    {
        return Task.FromResult(IdentityResult.Success);
    }
}
