using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;
using OrchardCore.Roles.Recipes;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Security.Services;

namespace OrchardCore.Roles.Deployment
{
    public class AllRolesDeploymentSource : IDeploymentSource
    {
        private readonly RoleManager<IRole> _roleManager;
        private readonly IRoleService _roleService;

        public AllRolesDeploymentSource(RoleManager<IRole> roleManager, IRoleService roleService)
        {
            _roleManager = roleManager;
            _roleService = roleService;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var allRolesStep = step as AllRolesDeploymentStep;

            if (allRolesStep == null)
            {
                return;
            }

            // Get all roles
            var allRoles = await _roleService.GetRolesAsync();
            var permissions = new JArray();
            var tasks = new List<Task>();

            foreach (var role in allRoles)
            {
                var currentRole = (Role)await _roleManager.FindByNameAsync(_roleManager.NormalizeKey(role.RoleName));

                if (currentRole != null)
                {
                    permissions.Add(JObject.FromObject(
                        new RolesStepRoleModel
                        {
                            Name = currentRole.RoleName,
                            Description = currentRole.RoleDescription,
                            Permissions = currentRole.RoleClaims.Where(x => x.ClaimType == Permission.ClaimType).Select(x => x.ClaimValue).ToArray()
                        }));
                }
            }

            result.Steps.Add(new JObject(
                new JProperty("name", "Roles"),
                new JProperty("Roles", permissions)
            ));
        }
    }
}
