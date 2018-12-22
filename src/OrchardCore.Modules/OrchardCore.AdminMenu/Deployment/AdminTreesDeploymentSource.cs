using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;

namespace OrchardCore.AdminMenu.Deployment
{
    public class AdminMenuDeploymentSource : IDeploymentSource
    {
        private readonly IAdminMenuervice _AdminMenuervice;

        public AdminMenuDeploymentSource(IAdminMenuervice AdminMenuervice)
        {
            _AdminMenuervice = AdminMenuervice;
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

            foreach (var adminTree in await _AdminMenuervice.GetAsync())
            {
                var objectData = JObject.FromObject(adminTree, serializer);                
                data.Add(objectData);
            }

            return;
        }
    }
}
