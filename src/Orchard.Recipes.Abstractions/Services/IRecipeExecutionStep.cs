using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Orchard.Recipes.Models;

namespace Orchard.Recipes.Services
{
    public interface IRecipeExecutionStep
    {
        string Name { get; }
        IEnumerable<string> Names { get; }
        LocalizedString DisplayName { get; }
        LocalizedString Description { get; }

        void Configure(RecipeExecutionStepConfigurationContext context);
        void UpdateStep(UpdateRecipeExecutionStepContext context);
        Task ExecuteAsync(RecipeExecutionContext context);
    }
}