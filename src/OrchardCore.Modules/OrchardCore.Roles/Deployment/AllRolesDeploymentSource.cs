using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Deployment;
using OrchardCore.Roles.Recipes;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Security.Services;

namespace OrchardCore.Roles.Deployment;

public class AllRolesDeploymentSource
    : DeploymentSourceBase<AllRolesDeploymentStep>
{
    private readonly RoleManager<IRole> _roleManager;
    private readonly IRoleService _roleService;

    public AllRolesDeploymentSource(
        RoleManager<IRole> roleManager,
        IRoleService roleService)
    {
        _roleManager = roleManager;
        _roleService = roleService;
    }

    protected override async Task ProcessAsync(AllRolesDeploymentStep step, DeploymentPlanResult result)
    {
        // Get all roles
        var allRoles = await _roleService.GetRolesAsync();
        var permissions = new JsonArray();
        var tasks = new List<Task>();

        foreach (var role in allRoles)
        {
            var currentRole = await _roleManager.FindByNameAsync(role.RoleName);

            if (currentRole is Role r)
            {
                permissions.Add(JObject.FromObject(
                    new RolesStepRoleModel
                    {
                        Name = r.RoleName,
                        Description = r.RoleDescription,
                        Permissions = r.RoleClaims.Where(x => x.ClaimType == Permission.ClaimType).Select(x => x.ClaimValue).ToArray()
                    }));
            }
        }

        result.Steps.Add(new JsonObject
        {
            ["name"] = "Roles",
            ["Roles"] = permissions,
        });
    }
}
