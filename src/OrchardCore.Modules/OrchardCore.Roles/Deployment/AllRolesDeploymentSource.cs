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
        private readonly IRoleProvider _roleProvider;

        public AllRolesDeploymentSource(RoleManager<IRole> roleManager, IRoleProvider roleProvider)
        {
            _roleManager = roleManager;
            _roleProvider = roleProvider;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var allRolesState = step as AllRolesDeploymentStep;

            if (allRolesState == null)
            {
                return;
            }

            // Get all roles
            var allRoleNames = await _roleProvider.GetRoleNamesAsync();
            var permissions = new JArray();
            var tasks = new List<Task>();

            foreach (var roleName in allRoleNames)
            {
                var task = _roleManager.FindByNameAsync(_roleManager.NormalizeKey(roleName)).ContinueWith(async roleTask =>
                {
                    var role = (Role)await roleTask;
                    if (role != null)
                    {
                        permissions.Add(JObject.FromObject(
                            new RolesStepRoleModel
                            {
                                Name = role.NormalizedRoleName,
                                Permissions = role.RoleClaims.Where(x => x.ClaimType == Permission.ClaimType).Select(x => x.ClaimValue).ToArray()
                            }));
                    }
                });

                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            result.Steps.Add(new JObject(
                new JProperty("name", "Roles"),
                new JProperty("Roles", permissions)
            ));
        }
    }
}
