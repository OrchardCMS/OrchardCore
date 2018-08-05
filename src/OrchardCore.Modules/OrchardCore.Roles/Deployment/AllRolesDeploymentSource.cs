using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;
using OrchardCore.Security;
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
            var data = new JArray();
            var tasks = new List<Task>();

            foreach(var roleName in allRoleNames)
            {
                var task = _roleManager.FindByNameAsync(_roleManager.NormalizeKey(roleName)).ContinueWith(async roleTask =>
                {
                    var role = (Role) await roleTask;
                    if (role != null)
                    {
                        data.Add(JObject.FromObject(role));
                    }
                });

                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            result.Steps.Add(new JObject(
                new JProperty("name", "Roles"),
                new JProperty("Data", data)
            ));
        }
    }
}
