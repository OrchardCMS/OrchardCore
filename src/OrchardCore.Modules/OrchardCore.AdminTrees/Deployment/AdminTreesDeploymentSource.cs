using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;

namespace OrchardCore.AdminTrees.Deployment
{
    public class AdminTreesDeploymentSource : IDeploymentSource
    {
        private readonly IAdminTreeService _adminTreeService;

        public AdminTreesDeploymentSource(IAdminTreeService adminTreeService)
        {
            _adminTreeService = adminTreeService;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var adminTreesState = step as AdminTreesDeploymentStep;

            if (adminTreesState == null)
            {
                return;
            }

            var data = new JArray();
            result.Steps.Add(new JObject(
                new JProperty("name", "AdminTrees"),
                new JProperty("data", data)
            ));

            // For each AdminNode, store info about its concrete type: linkAdminNode, contentTypesAdminNode etc...
            var serializer = new JsonSerializer() {  TypeNameHandling = TypeNameHandling.Auto };

            foreach (var adminTree in await _adminTreeService.GetAsync())
            {
                var objectData = JObject.FromObject(adminTree, serializer);                
                data.Add(objectData);
            }

            return;
        }
    }
}
