using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Recipes.Executors
{
    /// <summary>
    /// This step adds a set of features to any recipe that have a 'Feature' step, need to be registered first.
    /// </summary>
    public class AddRecipeFeaturesStep : IRecipeStepHandler
    {
        private readonly IEnumerable<string> _featureIds;

        public AddRecipeFeaturesStep(params string[] featureIds)
        {
            _featureIds = featureIds;
        }

        public Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (String.Equals(context.Name, "Feature", StringComparison.OrdinalIgnoreCase))
            {
                var step = context.Step.ToObject<FeatureStepModel>();
                step.Enable = step.Enable.Concat(_featureIds).ToArray();
                context.Step = JObject.FromObject(step);
            }

            return Task.CompletedTask;
        }
    }
}