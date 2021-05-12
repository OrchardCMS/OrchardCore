using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;
using OrchardCore.Placements.Services;

namespace OrchardCore.Placements.Deployment
{
    public class PlacementsDeploymentSource : IDeploymentSource
    {
        private readonly PlacementsManager _placementsManager;

        public PlacementsDeploymentSource(PlacementsManager placementsManager)
        {
            _placementsManager = placementsManager;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var placementsStep = step as PlacementsDeploymentStep;

            if (placementsStep == null)
            {
                return;
            }

            var placementObjects = new JObject();
            var placements = await _placementsManager.ListShapePlacementsAsync();

            foreach (var placement in placements)
            {
                placementObjects[placement.Key] = JArray.FromObject(placement.Value);
            }

            result.Steps.Add(new JObject(
                new JProperty("name", "Placements"),
                new JProperty("Placements", placementObjects)
            ));
        }
    }
}
