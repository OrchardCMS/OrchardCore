using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
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
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly IRoleService _roleService;

        public AllRolesDeploymentSource(
            RoleManager<IRole> roleManager,
            IOptions<JsonSerializerOptions> jsonSerializerOptions,
            IRoleService roleService)
        {
            _roleManager = roleManager;
            _jsonSerializerOptions = jsonSerializerOptions.Value;
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
            var permissions = new JsonArray();
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
                        }, _jsonSerializerOptions));
                }
            }

            result.Steps.Add(new JsonObject
            {
                ["name"] = "Roles",
                ["Roles"] = permissions,
            });
        }
    }
}
