using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
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

            var placementObjects = new JsonObject();
            var placements = await _placementsManager.ListShapePlacementsAsync();

            foreach (var placement in placements)
            {
                placementObjects[placement.Key] = JsonSerializer.SerializeToNode(placement.Value);
            }

            result.AddSimpleStep("Placements", "Placements", placementObjects);
        }
    }
}
