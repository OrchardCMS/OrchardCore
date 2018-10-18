using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
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

            var featuresToDisable = new List<IFeatureInfo>();

            if (step.Disable.Any())
            {
                featuresToDisable = features.Where(x => step.Disable.Contains(x.Id)).ToList();
            }

            var featuresToEnable = new List<IFeatureInfo>();

            if (step.Enable.Any())
            {
                featuresToEnable = features.Where(x => step.Enable.Contains(x.Id)).ToList();
            }

            if (featuresToDisable.Count > 0 && featuresToEnable.Count == 0)
            {
                return;
            }

            if (featuresToDisable.Count > 0 && featuresToEnable.Count > 0)
            {
                await _shellFeatureManager.DisableEnableFeaturesAsync(featuresToDisable, featuresToEnable, true);
            }

            else if (featuresToDisable.Count > 0)
            {
                await _shellFeatureManager.DisableFeaturesAsync(featuresToDisable, true);
            }

            else if (featuresToEnable.Count > 0)
            {
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