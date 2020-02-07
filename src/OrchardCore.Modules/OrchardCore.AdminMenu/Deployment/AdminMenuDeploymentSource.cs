using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;

namespace OrchardCore.AdminMenu.Deployment
{
    public class AdminMenuDeploymentSource : IDeploymentSource
    {
        private readonly IAdminMenuService _adminMenuService;

        public AdminMenuDeploymentSource(IAdminMenuService adminMenuervice)
        {
            _adminMenuService = adminMenuervice;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var AdminMenuState = step as AdminMenuDeploymentStep;

            if (AdminMenuState == null)
            {
                return;
            }

            var data = new JArray();
            result.Steps.Add(new JObject(
                new JProperty("name", "AdminMenu"),
                new JProperty("data", data)
            ));

            // For each AdminNode, store info about its concrete type: linkAdminNode, contentTypesAdminNode etc...
            var serializer = new JsonSerializer() { TypeNameHandling = TypeNameHandling.Auto };

            foreach (var adminMenu in (await _adminMenuService.GetAdminMenuListAsync()).AdminMenu)
            {
                var objectData = JObject.FromObject(adminMenu, serializer);
                data.Add(objectData);
            }

            return;
        }
    }
}
