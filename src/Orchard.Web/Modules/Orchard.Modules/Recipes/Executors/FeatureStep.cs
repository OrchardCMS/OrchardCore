using System;
using System.Linq;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using Orchard.Environment.Extensions.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Localization;

namespace Orchard.Modules.Recipes.Executors
{
    public class FeatureStep : RecipeExecutionStep
    {
        private readonly IFeatureManager _featureManager;

        public FeatureStep(
            IFeatureManager featureManager,
            ILoggerFactory logger,
            IStringLocalizer<FeatureStep> localizer) : base(logger, localizer)
        {

            _featureManager = featureManager;
        }

        public override string Name
        {
            get { return "Feature"; }
        }

        public override void Execute(RecipeExecutionContext recipeContext)
        {
            var step = recipeContext.RecipeStep.Step.ToObject<InternalStep>();

            var availableFeatures = _featureManager.GetAvailableFeaturesAsync().Result.Select(x => x.Id).ToArray();
            foreach (var featureName in step.Disable)
            {
                if (!availableFeatures.Contains(featureName))
                {
                    throw new InvalidOperationException(string.Format("Could not disable feature {0} because it was not found.", featureName));
                }
            }

            foreach (var featureName in step.Enable)
            {
                if (!availableFeatures.Contains(featureName))
                {
                    throw new InvalidOperationException(string.Format("Could not enable feature {0} because it was not found.", featureName));
                }
            }

            if (step.Disable.Any())
            {
                Logger.LogInformation("Disabling features: {0}", string.Join(";", step.Disable));
                _featureManager.DisableFeaturesAsync(step.Disable, true);
            }
            if (step.Enable.Any())
            {
                Logger.LogInformation("Enabling features: {0}", string.Join(";", step.Enable));
                _featureManager.EnableFeaturesAsync(step.Enable, true);
            }
        }

        private class InternalStep {
            public string Name { get; set; }
            public string[] Disable { get; set; }
            public string[] Enable { get; set; }
        }
    }
}