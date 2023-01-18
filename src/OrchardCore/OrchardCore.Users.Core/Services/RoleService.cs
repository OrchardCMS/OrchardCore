using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Security;
using OrchardCore.Security.Services;

namespace OrchardCore.Roles.Services;

public class RoleService : IRoleService
{
    private readonly RoleManager<IRole> _roleManager;

    public RoleService(RoleManager<IRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task<IEnumerable<Claim>> GetRoleClaimsAsync(string role, CancellationToken cancellationToken = default)
    {
        if (String.IsNullOrEmpty(role))
        {
            throw new ArgumentException("The role name cannot be null or empty.", nameof(role));
        }

        var entity = await _roleManager.FindByNameAsync(role);
        if (entity == null)
        {
            return Array.Empty<Claim>();
        }

        return await _roleManager.GetClaimsAsync(entity);
    }

    public Task<IEnumerable<IRole>> GetRolesAsync()
    {
        return Task.FromResult<IEnumerable<IRole>>(_roleManager.Roles);
    }

    public Task<IEnumerable<string>> GetRoleNamesAsync()
    {
        return Task.FromResult<IEnumerable<string>>(_roleManager.Roles.Select(a => a.RoleName));
    }

    public Task<IEnumerable<string>> GetNormalizedRoleNamesAsync()
    {
        return Task.FromResult<IEnumerable<string>>(_roleManager.Roles.Select(a => _roleManager.NormalizeKey(a.RoleName)));
    }
}
