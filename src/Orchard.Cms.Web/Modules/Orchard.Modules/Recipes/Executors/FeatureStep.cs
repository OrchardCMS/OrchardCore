using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Extensions.Features;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using Orchard.Environment.Extensions;
using Orchard.Environment.Shell;

namespace Orchard.Modules.Recipes.Executors
{
    public class FeatureStep : RecipeExecutionStep
    {
        private readonly IExtensionManager _extensionManager;
        private readonly IShellFeaturesManager _shellFeatureManager;

        public FeatureStep(
            IExtensionManager extensionManager,
            IShellFeaturesManager shellFeatureManager,
            ILoggerFactory logger,
            IStringLocalizer<FeatureStep> localizer) : base(logger, localizer)
        {
            _extensionManager = extensionManager;
            _shellFeatureManager = shellFeatureManager;
        }

        public override string Name
        {
            get { return "Feature"; }
        }

        public override Task ExecuteAsync(RecipeExecutionContext recipeContext)
        {
            var step = recipeContext.RecipeStep.Step.ToObject<InternalStep>();

            var extensions = _extensionManager.GetExtensions();

            foreach (var featureName in step.Disable)
            {
                if (!extensions.HasFeature(featureName))
                {
                    throw new InvalidOperationException(string.Format("Could not disable feature {0} because it was not found.", featureName));
                }
            }

            foreach (var featureName in step.Enable)
            {
                if (!extensions.HasFeature(featureName))
                {
                    throw new InvalidOperationException(string.Format("Could not enable feature {0} because it was not found.", featureName));
                }
            }

            if (step.Disable.Any())
            {
                if (Logger.IsEnabled(LogLevel.Information))
                {
                    Logger.LogInformation("Disabling features: {0}", string.Join(";", step.Disable));
                }

                var featuresToDisable = step.Disable.Select(d => extensions.Features.First(x => x.Id == d));

                _shellFeatureManager.DisableFeatures(featuresToDisable, true);
            }

            if (step.Enable.Any())
            {
                if (Logger.IsEnabled(LogLevel.Information))
                {
                    Logger.LogInformation("Enabling features: {0}", string.Join(";", step.Enable));
                }

                var featuresToEnable = step.Enable.Select(d => extensions.Features.First(x => x.Id == d));

                _shellFeatureManager.EnableFeatures(featuresToEnable, true);
            }

            return Task.CompletedTask;
        }

        private class InternalStep {
            public string Name { get; set; }
            public string[] Disable { get; set; }
            public string[] Enable { get; set; }
        }
    }
}