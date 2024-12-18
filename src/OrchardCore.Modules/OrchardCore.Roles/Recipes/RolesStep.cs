using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Roles.Recipes;

/// <summary>
/// This recipe step creates a set of roles.
/// </summary>
public sealed class RolesStep : NamedRecipeStepHandler
{
    private readonly RoleManager<IRole> _roleManager;
    private readonly ISystemRoleNameProvider _systemRoleNameProvider;

    public RolesStep(
        RoleManager<IRole> roleManager,
        ISystemRoleNameProvider systemRoleNameProvider)
        : base("Roles")
    {
        _roleManager = roleManager;
        _systemRoleNameProvider = systemRoleNameProvider;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var model = context.Step.ToObject<RolesStepModel>();

        foreach (var roleEntry in model.Roles)
        {
            var roleName = roleEntry.Name?.Trim();

            if (string.IsNullOrWhiteSpace(roleName))
            {
                continue;
            }

            var role = await _roleManager.FindByNameAsync(roleName);
            var isNewRole = role == null;

            if (isNewRole)
            {
                role = new Role
                {
                    RoleName = roleName,
                };
            }

            if (role is Role r)
            {
                r.RoleDescription = roleEntry.Description;

                if (roleEntry.PermissionBehavior == PermissionBehavior.Replace)
                {
                    // At this point, we know we are replacing permissions.
                    // Remove all existing permission so we can add the replacements later.
                    r.RoleClaims.RemoveAll(c => c.ClaimType == Permission.ClaimType);
                }

                if (!await _systemRoleNameProvider.IsAdminRoleAsync(roleName))
                {
                    if (roleEntry.PermissionBehavior == PermissionBehavior.Remove)
                    {
                        // Materialize this list to prevent an exception. 
                        var permissions = r.RoleClaims.Where(c => c.ClaimType == Permission.ClaimType && roleEntry.Permissions.Contains(c.ClaimValue)).ToArray();

                        foreach (var permission in permissions)
                        {
                            r.RoleClaims.Remove(permission);
                        }
                    }
                    else
                    {
                        var permissions = roleEntry.Permissions.Select(RoleClaim.Create)
                            .Where(newClaim => !r.RoleClaims.Exists(existingClaim => existingClaim.ClaimType == newClaim.ClaimType && existingClaim.ClaimValue == newClaim.ClaimValue));

                        r.RoleClaims.AddRange(permissions);
                    }
                }
            }

            if (isNewRole)
            {
                await _roleManager.CreateAsync(role);
            }
            else
            {
                await _roleManager.UpdateAsync(role);
            }
        }
    }
}

public sealed class RolesStepModel
{
    public RolesStepRoleModel[] Roles { get; set; }
}

public sealed class RolesStepRoleModel
{
    public string Name { get; set; }

    public string Description { get; set; }

    public string[] Permissions { get; set; }

    public PermissionBehavior PermissionBehavior { get; set; }
}

public enum PermissionBehavior
{
    Replace,
    Add,
    Remove,
}
