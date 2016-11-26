using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Extensions;
using Orchard.Environment.Shell;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

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

        public override async Task ExecuteAsync(RecipeExecutionContext recipeContext)
        {
            var step = recipeContext.RecipeStep.Step.ToObject<InternalStep>();

            var extensions = _extensionManager.GetExtensions();
            var features = extensions.Features;

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
                if (Logger.IsEnabled(LogLevel.Information))
                {
                    Logger.LogInformation("Disabling features: {0}", string.Join(";", step.Disable));
                }

                var featuresToDisable = features.Where(x => step.Disable.Contains(x.Id)).ToList();

                await _shellFeatureManager.DisableFeaturesAsync(featuresToDisable, true);
            }

            if (step.Enable.Any())
            {
                if (Logger.IsEnabled(LogLevel.Information))
                {
                    Logger.LogInformation("Enabling features: {0}", string.Join(";", step.Enable));
                }

                var featuresToEnable = extensions.Features.Where(x => step.Enable.Contains(x.Id)).ToList();

                await _shellFeatureManager.EnableFeaturesAsync(featuresToEnable, true);
            }

            await Task.CompletedTask;
        }

        private class InternalStep {
            public string Name { get; set; }
            public string[] Disable { get; set; }
            public string[] Enable { get; set; }
        }
    }
}