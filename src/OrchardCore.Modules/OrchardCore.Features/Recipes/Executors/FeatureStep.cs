using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Features.Recipes.Executors
{
    /// <summary>
    /// This recipe step enables or disables a set of features.
    /// </summary>
    public class FeatureStep : IRecipeStepHandler
    {
        private readonly IShellFeaturesManager _shellFeaturesManager;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public FeatureStep(
            IShellFeaturesManager shellFeaturesManager,
            IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _shellFeaturesManager = shellFeaturesManager;
            _jsonSerializerOptions = jsonSerializerOptions.Value;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, "Feature", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var step = context.Step.ToObject<FeatureStepModel>(_jsonSerializerOptions);

            var features = (await _shellFeaturesManager.GetAvailableFeaturesAsync());

            var featuresToDisable = features.Where(x => step.Disable?.Contains(x.Id) == true).ToList();
            var featuresToEnable = features.Where(x => step.Enable?.Contains(x.Id) == true).ToList();

            if (featuresToDisable.Count > 0 || featuresToEnable.Count > 0)
            {
                await _shellFeaturesManager.UpdateFeaturesAsync(featuresToDisable, featuresToEnable, true);
            }
        }

        private class FeatureStepModel
        {
            public string Name { get; set; }
            public string[] Disable { get; set; }
            public string[] Enable { get; set; }
        }
    }
}
