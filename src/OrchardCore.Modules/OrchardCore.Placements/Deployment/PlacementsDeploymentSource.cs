using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Deployment;
using OrchardCore.Placements.Services;

namespace OrchardCore.Placements.Deployment
{
    public class PlacementsDeploymentSource : IDeploymentSource
    {
        private readonly PlacementsManager _placementsManager;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public PlacementsDeploymentSource(
            PlacementsManager placementsManager,
            IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _placementsManager = placementsManager;
            _jsonSerializerOptions = jsonSerializerOptions.Value;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            if (step is not PlacementsDeploymentStep)
            {
                return;
            }

            var placementObjects = new JsonObject();
            var placements = await _placementsManager.ListShapePlacementsAsync();

            foreach (var placement in placements)
            {
                placementObjects[placement.Key] = JArray.FromObject(placement.Value, _jsonSerializerOptions);
            }

            result.Steps.Add(new JsonObject
            {
                ["name"] = "Placements",
                ["Placements"] = placementObjects,
            });
        }
    }
}
