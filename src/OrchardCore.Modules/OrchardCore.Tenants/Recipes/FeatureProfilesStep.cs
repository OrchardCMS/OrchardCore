using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Tenants.Services;

namespace OrchardCore.Tenants.Recipes
{
    /// <summary>
    /// This recipe step creates a set of feature profiles.
    /// </summary>
    public class FeatureProfilesStep : IRecipeStepHandler
    {
        private readonly FeatureProfilesManager _featureProfilesManager;

        public FeatureProfilesStep(FeatureProfilesManager featureProfilesManager)
        {
            _featureProfilesManager = featureProfilesManager;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (context.TryGetStepPropertyIfNameMatches<FeatureProfile>("FeatureProfiles", out var featureProfiles))
            {
                foreach (var (name, value) in featureProfiles)
                {
                    await _featureProfilesManager.UpdateFeatureProfileAsync(name, value);
                }
            }
        }
    }
}
