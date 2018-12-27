using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;

namespace OrchardCore.AdminMenu.Deployment
{
    public class AdminMenuDeploymentSource : IDeploymentSource
    {
        private readonly IAdminMenuService _AdminMenuService;

        public AdminMenuDeploymentSource(IAdminMenuService AdminMenuervice)
        {
            _AdminMenuService = AdminMenuervice;
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
            var serializer = new JsonSerializer() {  TypeNameHandling = TypeNameHandling.Auto };

            foreach (var adminMenu in await _AdminMenuService.GetAsync())
            {
                var objectData = JObject.FromObject(adminMenu, serializer);                
                data.Add(objectData);
            }

            return;
        }
    }
}
