using System;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Environment.Extensions;
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
        private readonly IExtensionManager _extensionManager;
        private readonly IShellFeaturesManager _shellFeatureManager;

        public FeatureStep(
            IExtensionManager extensionManager,
            IShellFeaturesManager shellFeatureManager)
        {
            _extensionManager = extensionManager;
            _shellFeatureManager = shellFeatureManager;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, "Feature", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var step = context.Step.ToObject<FeatureStepModel>();

            var features = _extensionManager.GetFeatures();

            foreach (var featureId in step.Disable)
            {
                if (features.Any(x => x.Id == featureId))
                {
                    throw new InvalidOperationException(string.Format("Could not disable feature {0} because it was not found.", featureId));
                }
            }

            foreach (var featureId in step.Enable)
            {
                if (!features.Any(x => x.Id == featureId))
                {
                    throw new InvalidOperationException(string.Format("Could not enable feature {0} because it was not found.", featureId));
                }
            }

            if (step.Disable.Any())
            {
                var featuresToDisable = features.Where(x => step.Disable.Contains(x.Id)).ToList();

                await _shellFeatureManager.DisableFeaturesAsync(featuresToDisable, true);
            }

            if (step.Enable.Any())
            {
                var featuresToEnable = features.Where(x => step.Enable.Contains(x.Id)).ToList();

                await _shellFeatureManager.EnableFeaturesAsync(featuresToEnable, true);
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