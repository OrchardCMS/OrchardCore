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
            if (!String.Equals(context.Name, "FeatureProfiles", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (context.Step.Property("FeatureProfiles")?.Value is JObject featureProfiles)
            {
                foreach (var property in featureProfiles.Properties())
                {
                    var name = property.Name;
                    var value = property.Value.ToObject<FeatureProfile>();

                    await _featureProfilesManager.UpdateFeatureProfileAsync(name, value);
                }
            }
        }
    }
}
